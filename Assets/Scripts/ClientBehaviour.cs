using System.Text;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Logging;

public class ClientBehaviour : MonoBehaviour
{
    public NetworkDriver Driver;
    private NativeList<NetworkConnection> m_Connections;
    public bool done;

    private Dictionary<uint, BallScript> m_Balls = new();

    private void Awake()
    {
        // Load all the grid
        for (int i = 1; i <= 9; i++)
        {
            SceneManager.LoadScene(i, LoadSceneMode.Additive);
        }

        // Connect to all the servers
        var settings = new NetworkSettings();
        settings.WithNetworkConfigParameters(maxFrameTimeMS: 100, receiveQueueCapacity: ushort.MaxValue, sendQueueCapacity: ushort.MaxValue);
        Driver = NetworkDriver.Create(settings);

        var clientEndpoint = NetworkEndpoint.LoopbackIpv4;
        clientEndpoint.Port = (ushort)(20000 + Bootstrap.ClientIndex);
        Driver.Bind(clientEndpoint);

        // Try to log processID - endpoint
        StartCoroutine(HelperFunctions.LogProcessIDEndpoint());

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

    private void Start()
    {
        var ballScripts = FindObjectsOfType<BallScript>();
        foreach (var ballScript in ballScripts)
        {
            m_Balls[ballScript.id] = ballScript;
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

    private void FixedUpdate()
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

        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                continue;
            }

            NetworkEvent.Type cmd;
            while ((cmd = Driver.PopEventForConnection(m_Connections[i], out var reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Log.Debug("We are now connected to the server");

                    // Sent ping (client index)
                    SendPingToConnection(m_Connections[i]);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var msgByte = reader.ReadByte();
                    var msgType = (MessageType)msgByte;
                    switch (msgType)
                    {
                        default:
                        case MessageType.Unknown:
                            Log.Warning("Client received an unexpected message! (type: {0})", msgType.ToString());
                            break;
                        case MessageType.BallUpdate:
                        {
                            int isServer = reader.ReadInt();
                            int serverIndex = reader.ReadInt();

                            if (isServer != 1)
                            {
                                Log.Warning("Client has received data from another client!");
                                continue;
                            }

                            ParseNetworkBallDataFromStream(reader);
                            break;
                        }
                    }

                    // Sent ping (client index) back
                    SendPingToConnection(m_Connections[i]);

                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Log.Debug("Client disconnected from server");
                    m_Connections[i] = default;
                }
            }
        }
    }

    private void SendPingToConnection(NetworkConnection networkConnection)
    {
        Driver.BeginSend(NetworkPipeline.Null, networkConnection, out var writer);
        if (!writer.IsCreated)
        {
            return;
        }

        writer.WriteByte((byte)MessageType.Ping);
        writer.WriteInt(0); // indicates that this is a client
        writer.WriteInt(Bootstrap.ClientIndex);
        Driver.EndSend(writer);
    }

    private void ParseNetworkBallDataFromStream(DataStreamReader reader)
    {
        // Receive ball data
        int ballCount = reader.ReadInt();
        for (int i = 0; i < ballCount; i++)
        {
            var networkBallData = new NetworkBallData();
            networkBallData.Deserialize(ref reader);

            // Update corresponding ball's location/rotation
            var ballTransform = m_Balls[networkBallData.ID].transform;
            ballTransform.SetPositionAndRotation(new Vector3(networkBallData.PositionX, networkBallData.PositionY, networkBallData.PositionZ),
                new Quaternion(networkBallData.RotationX, networkBallData.RotationY, networkBallData.RotationZ, networkBallData.RotationW));
        }
    }
}
