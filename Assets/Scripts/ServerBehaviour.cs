using System.Collections;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Logging;

public class ServerBehaviour : MonoBehaviour
{
    [SerializeField]
    private BallScript ballPrefab = default;

    public NetworkDriver Driver;
    private NativeList<NetworkConnection> m_Connections;

    private Bounds m_Bounds;

    private void OnDestroy()
    {
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
            // This is a server. Set up server.

            // Calculate boundaries
            m_Bounds = GetComponentInChildren<MeshRenderer>().bounds;
            m_Bounds.Expand(new Vector3(0f, 20f, 0f));

            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(maxFrameTimeMS: 100, receiveQueueCapacity: ushort.MaxValue, sendQueueCapacity: ushort.MaxValue);
            Driver = NetworkDriver.Create(settings);
            var endpoint = NetworkEndpoint.AnyIpv4;
            endpoint.Port = 10000;
            endpoint.Port += (ushort)Bootstrap.ServerIndex;

            // Try to log processID - endpoint
            StartCoroutine(HelperFunctions.LogProcessIDEndpoint());

            // Bind to the related network port, start listening
            if (Driver.Bind(endpoint) != 0)
            {
                Log.Debug("Failed to bind to port " + endpoint.Port);
            }
            else
            {
                Driver.Listen();
            }

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

            // Connect to other servers,
            endpoint = NetworkEndpoint.LoopbackIpv4;
            for (ushort i = 0; i < Bootstrap.ServerIndex; i++)
            {
                endpoint.Port = 10000;
                endpoint.Port += i;
                m_Connections.Add(Driver.Connect(endpoint));
            }

            StartCoroutine(MakeBallsNonKinematicWithDelay());
        }
        else
        {
            // This is NOT a server! Destroy this GameObject
            //Destroy(gameObject);
            enabled = false;
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

        // Accept new connections
        NetworkConnection c;
        while ((c = Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Log.Debug("Accepted a connection");
        }

        // Check m_Connections
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
                    // Connect event should only be encountered on a connection to a server
                    // This is a server connection.
                    Log.Debug("We are now connected to another server");
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var msgByte = reader.ReadByte();
                    var msgType = (MessageType)msgByte;
                    switch (msgType)
                    {
                        default:
                        case MessageType.Unknown:
                            Log.Warning("Server received an unexpected message! (type: {0})", msgType.ToString());
                            break;
                        case MessageType.Ping:
                        {
                            break;
                        }
                        case MessageType.BallUpdate:
                        {
                            int isServer = reader.ReadInt();
                            int serverIndex = reader.ReadInt();

                            if (isServer == 1)
                            {
                                // Parse received ball data
                                ParseNetworkBallDataFromStream(reader);
                            }

                            break;
                        }
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Log.Debug("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }

        // Send all ball data to all connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                continue;
            }

            // Send back ball data
            SendNetworkBallDataToConnection(m_Connections[i]);
        }

        // All ball data is sent, destroy balls that has left our bounds
        // DestroyBallsNotWithinBounds();
    }

    private void SendNetworkBallDataToConnection(NetworkConnection networkConnection)
    {
        // Begin message with isServer information and m_BallScripts count
        Driver.BeginSend(NetworkPipeline.Null, networkConnection, out var writer);
        if (!writer.IsCreated)
        {
            return;
        }

        writer.WriteByte((byte)MessageType.BallUpdate);
        writer.WriteInt(Bootstrap.IsServer ? 1 : 0);
        writer.WriteInt(Bootstrap.ServerIndex);
        writer.WriteInt(BallScript.Instances.Length);

        foreach (var ballScript in BallScript.Instances)
        {
            // Create new NetworkBallData from ballScript
            NetworkBallData networkBallData = new NetworkBallData();
            networkBallData.ID = ballScript.id;

            Vector3 ballPosition = ballScript.transform.position;
            networkBallData.PositionX = ballPosition.x;
            networkBallData.PositionY = ballPosition.y;
            networkBallData.PositionZ = ballPosition.z;

            Quaternion ballRotation = ballScript.transform.rotation;
            networkBallData.RotationX = ballRotation.x;
            networkBallData.RotationY = ballRotation.y;
            networkBallData.RotationZ = ballRotation.z;
            networkBallData.RotationW = ballRotation.w;

            Vector3 ballVelocity = ballScript.rb.velocity;
            networkBallData.VelocityX = ballVelocity.x;
            networkBallData.VelocityY = ballVelocity.y;
            networkBallData.VelocityZ = ballVelocity.z;

            networkBallData.Serialize(ref writer);
        }

        // Send to connection
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

            /*// If the received ball data is within our boundaries, start simulating it as a new ball
            Vector3 ballPosition = new Vector3(networkBallData.positionX, networkBallData.positionY, networkBallData.positionZ);
            if (m_Bounds.Contains(ballPosition))
            {
                Quaternion ballRotation = new Quaternion(networkBallData.rotationX, networkBallData.rotationY, networkBallData.rotationZ, networkBallData.rotationW);
                BallScript newBall = Instantiate(ballPrefab, ballPosition, ballRotation, null);
                newBall.id = networkBallData.id;
                newBall.rb.isKinematic = false;
                newBall.rb.velocity = new Vector3(networkBallData.velocityX, networkBallData.velocityY, networkBallData.velocityZ);
                m_BallScripts.Add(newBall);
                Log.Debug("Instantiated new ball");
            }*/
        }
    }

    /*private void DestroyBallsNotWithinBounds()
    {
        for (int i = 0; i < m_BallScripts.Count; i++)
        {
            if (m_Bounds.Contains(m_BallScripts[i].transform.position))
            {
                // This ball is still within our bounds, continue
            }
            else
            {
                // This ball has left our bounds, destroy it from this server's perspective
                Log.Debug("Destroyed ball");
                Destroy(m_BallScripts[i].gameObject);
                m_BallScripts.RemoveAtSwapBack(i);
                i--;
            }
        }
    }*/

    private IEnumerator MakeBallsNonKinematicWithDelay()
    {
        yield return new WaitForSeconds(1f);

        foreach (var ballScript in BallScript.Instances)
        {
            ballScript.rb.isKinematic = false;
        }
    }
}
