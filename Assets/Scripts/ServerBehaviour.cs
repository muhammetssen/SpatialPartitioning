using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Text;
using System.Diagnostics;

using Unity.Collections;
using Unity.Networking.Transport;

using Unity.Logging;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver Driver;

    private NativeList<NetworkConnection> m_Connections;
    private NativeArray<byte> m_TempBuffer;

    private void Awake()
    {
        m_TempBuffer = new(1024, Allocator.Persistent);
        for (int x = 0; x < 1024; x++)
        {
            m_TempBuffer[x] = (byte)'x';
        }
    }

    private void OnDestroy()
    {
        if (m_TempBuffer.IsCreated)
        {
            m_TempBuffer.Dispose();
            m_TempBuffer = default;
        }

        if (Driver.IsCreated)
        {
            Driver.Dispose();
            m_Connections.Dispose();
        }
    }

    private void Start()
    {
        if (Bootstrap.IsServer)
        {
            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(maxFrameTimeMS: 100);
            // This is a server. Set up server.
            Driver = NetworkDriver.Create(settings);
            var endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 10000;
            endpoint.Port += Bootstrap.ServerIndex;
            if (Driver.Bind(endpoint) != 0)
            {
                Log.Debug("Failed to bind to port " + endpoint.Port);
            }
            else
            {
                Driver.Listen();
            }

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            // Log Process ID, Instance Type and Assigned Index
            Log.Debug($"Process ID: {Process.GetCurrentProcess().Id}\n" +
                $"Instance Type: Server\n" +
                $"Assigned Index: {Bootstrap.ServerIndex}");
        }
        else
        {
            // This is NOT a server! Destroy this GameObject
            Destroy(gameObject);
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

        // Accept new connections
        NetworkConnection c;
        while ((c = Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Log.Debug("Accepted a connection");
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
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();
                    Log.Debug("Client #" + number + " sent ping, timestamp " + Time.time);

                    /* var pingStr = $"I am server #{Bootstrap.ServerIndex} pinging you!";
                    var pingArr = Encoding.UTF8.GetBytes(pingStr);
                    var pingLen = pingArr.Length;
                    var pingNarr = new NativeArray<byte>(pingArr, Allocator.Temp); */

                    // Send back server index
                    Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteUInt(Bootstrap.ServerIndex);
                    writer.WriteBytes(m_TempBuffer);
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