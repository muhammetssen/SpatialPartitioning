using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> m_Connections;
    public bool done;

    public uint clientIndex = 0;

    private void Awake()
    {
        // Load all the grid
        for (int i = 1; i <= 9; i++)
        {
            SceneManager.LoadScene(i, LoadSceneMode.Additive);
        }

        // Connect to all the servers
        Driver = NetworkDriver.Create();
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        var endpoint = NetworkEndpoint.LoopbackIpv4;
        for (ushort i = 0; i < 9; i++)
        {
            endpoint.Port = 10000;
            endpoint.Port += i;
            m_Connections.Add(Driver.Connect(endpoint));
            //m_Connections[i] = Driver.Connect(endpoint);
        }
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
                    Debug.Log("We are now connected to the server");

                    // Sent ping (client index)
                    Driver.BeginSend(m_Connections[i], out var writer);
                    writer.WriteUInt(clientIndex);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();

                    int pingLen = stream.ReadInt();
                    var pingNarr = new NativeArray<byte>(pingLen, Allocator.Temp);
                    stream.ReadBytes(pingNarr);
                    var pingStr = Encoding.UTF8.GetString(pingNarr.ToArray());
                    Debug.Log(pingStr);
                    // Debug.Log("Server #" + number + " sent ping, timestamp " + Time.time);
                    pingNarr.Dispose();

                    // Sent ping (client index) back
                    Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteUInt(clientIndex);
                    Driver.EndSend(writer);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}
