using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Diagnostics;

using Unity.Logging;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> m_Connections;
    public bool done;

    private void Awake()
    {
        // Load all the grid
        for (int i = 1; i <= 9; i++)
        {
            SceneManager.LoadScene(i, LoadSceneMode.Additive);
        }

        // Connect to all the servers
        var settings = new NetworkSettings();
        settings.WithNetworkConfigParameters(maxFrameTimeMS: 100);
        Driver = NetworkDriver.Create(settings);

        var clientEndpoint = NetworkEndpoint.LoopbackIpv4;
        clientEndpoint.Port = (ushort)(20000 + Bootstrap.ClientIndex);
        Driver.Bind(clientEndpoint);

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        var endpoint = NetworkEndpoint.LoopbackIpv4;
        for (ushort i = 0; i < 9; i++)
        {
            endpoint.Port = 10000;
            endpoint.Port += i;
            m_Connections.Add(Driver.Connect(endpoint));
            //m_Connections[i] = Driver.Connect(endpoint);
        }

        // Log Process ID, Instance Type and Assigned Index
        Log.Debug($"Process ID: {Process.GetCurrentProcess().Id}\n" +
            $"Instance Type: Client\n" +
            $"Assigned Index: {Bootstrap.ClientIndex}");
    }

    private void OnDestroy()
    {
        if (Driver.IsCreated)
        {
            Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    private void Update()
    {
        Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                continue;
            }

            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Log.Debug("We are now connected to the server");

                    // Sent ping (client index)
                    Driver.BeginSend(m_Connections[i], out var writer);
                    writer.WriteUInt(Bootstrap.ClientIndex);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    /* uint number = stream.ReadUInt();

                    int pingLen = stream.ReadInt();
                    var pingNarr = new NativeArray<byte>(pingLen, Allocator.Temp);
                    stream.ReadBytes(pingNarr);
                    var pingStr = Encoding.UTF8.GetString(pingNarr.ToArray());
                    Log.Debug(pingStr);
                    // Log.Debug("Server #" + number + " sent ping, timestamp " + Time.time);
                    pingNarr.Dispose(); */

                    // Sent ping (client index) back
                    Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteUInt(Bootstrap.ClientIndex);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Log.Debug("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}
