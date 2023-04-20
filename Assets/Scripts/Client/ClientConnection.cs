using Unity.Networking.Transport;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections;

public enum ServerToClientMessages : byte
{
    Ping, // hello, are you there? Here is my position
    ObjectUpdate, // position of an object
    ServerChange, // connect to another server
    Unknown,
    Welcome, // welcome to the server, here is your id and the location you should be at
    ObjectRemoval, // remove an object
}

public class ClientConnection : MonoBehaviour
{
    public NetworkDriver m1_Driver;
    public NetworkConnection connection;
    private Dictionary<uint, GameObject> objects = new Dictionary<uint, GameObject>();

    public static uint serverIndex = 0;
    private uint id = 0;
    void Start()
    {
        m1_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = Config.GetClient2ServerPort(serverIndex);
        connection = default(NetworkConnection);
        connection = m1_Driver.Connect(endpoint);
        if (!connection.IsCreated)
            Debug.Log("Failed to create connection to endpoint");
        Debug.Log($"Client: Connected to {endpoint.Port}");

        StartCoroutine(SendPingToServer());
    }

    public void OnDestroy()
    {
        m1_Driver.Dispose();
    }
    void FixedUpdate()
    {
    }

    void Update()
    {
        m1_Driver.ScheduleUpdate().Complete();

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(m1_Driver, out var data)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                RegisterMe();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                ServerToClientMessages messageType = (ServerToClientMessages)data.ReadByte();
                switch (messageType)
                {
                    case ServerToClientMessages.Ping:
                        Debug.Log("Client: Received ping from server");
                        break;



                    case ServerToClientMessages.Welcome:
                        // Debug.Log("Client: Received welcome from server");
                        this.id = data.ReadUInt();
                        Debug.Log($"Client: Received welcome from server: {this.id}");
                        break;



                    case ServerToClientMessages.ObjectUpdate:
                        // Debug.Log("Client: Received object update from server");
                        var serializedObject = new SerializableObject();
                        // Debug.Log($"Client: Received object update from server: {serializedObject.id}");
                        serializedObject.Deserialize(ref data);
                        if (!objects.ContainsKey(serializedObject.id))
                        {
                            var cube = Instantiate(Resources.Load<GameObject>("Dummy"));
                            // cube.transform.parent = this.transform;
                            var dummy = cube.GetComponent<Dummy>();
                            dummy.SetText(serializedObject.id.ToString());
                            objects.Add(serializedObject.id, cube);
                        }
                        objects[serializedObject.id].transform.position = new Vector3(serializedObject.PositionX, serializedObject.PositionY, serializedObject.PositionZ);
                        break;



                    case ServerToClientMessages.ServerChange:
                        uint newServerId = data.ReadUInt();
                        var endpoint = NetworkEndPoint.LoopbackIpv4;
                        endpoint.Port = (ushort)Config.GetClient2ServerPort(newServerId);

                        connection = default(NetworkConnection);
                        connection.Disconnect(m1_Driver);

                        connection = m1_Driver.Connect(endpoint);
                        ClientConnection.serverIndex = newServerId;
                        break;
                    
                    case ServerToClientMessages.ObjectRemoval:
                        uint objectId = data.ReadUInt();
                        if (objects.ContainsKey(objectId))
                        {
                            Destroy(objects[objectId]);
                            objects.Remove(objectId);
                        }
                        break;

                    default:
                        Debug.Log("Client: Received unknown message from server");
                        break;
                }
                // Debug.Log("Client: Send Ping to Server");
                // SendPingToServer();

            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client: Client got disconnected from server");
                connection.Disconnect(m1_Driver);
            }
        }

    }

    private IEnumerator SendPingToServer()
    {
        while (true)
        {
            if (!connection.IsCreated) yield return new WaitForSeconds(1);
            try
            {
                m1_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
                if (!writer.IsCreated)
                {
                    Debug.LogError("Writer is not created");
                    throw new System.Exception("Writer is not created");
                }
                writer.WriteByte((byte)ClientToServerMessages.Ping);
                // send id
                writer.WriteUInt(this.id);
                // send current location
                writer.WriteFloat(transform.position.x);
                writer.WriteFloat(transform.position.y);
                writer.WriteFloat(transform.position.z);
                // Debug.Log("Client: Sent Ping to Server");
                m1_Driver.EndSend(writer);
            }
            catch (System.Exception ex)
            {

                Debug.LogException(ex);
                // throw;
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    private void RegisterMe()
    {
        m1_Driver.BeginSend(NetworkPipeline.Null, connection, out var writer);
        writer.WriteByte((byte)ClientToServerMessages.RegisterMe);
        // send current location
        writer.WriteFloat(transform.position.x);
        writer.WriteFloat(transform.position.y);
        writer.WriteFloat(transform.position.z);
        m1_Driver.EndSend(writer);
    }
}