using System;
using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using Unity.Networking.Transport;

public class ServerBehaviour : MonoBehaviour
{
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> m_Connections;

    private void Start()
    {
        if (Bootstrap.IsServer)
        {
            // This is a server. Set up server.
            Driver = NetworkDriver.Create();
            var endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 10000;
            endpoint.Port += Bootstrap.ServerIndex;
            if (Driver.Bind(endpoint) != 0)
            {
                Debug.Log("Failed to bind to port " + endpoint.Port);
            }
            else
            {
                Driver.Listen();
            }

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }
        else
        {
            // This is NOT a server! Destroy this GameObject
            Destroy(gameObject);
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

        // Accept new connections
        NetworkConnection c;
        while ((c = Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");
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
                    Debug.Log("Client #" + number + " sent ping, timestamp " + Time.time);

                    // Send back server index
                    Driver.BeginSend(NetworkPipeline.Null, m_Connections[i], out var writer);
                    writer.WriteUInt(Bootstrap.ServerIndex);
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
