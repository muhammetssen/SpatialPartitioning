using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public enum ServerToServerMessages : byte
{
    Ping, // hello, are you there?
    BroadcastEvent, // tell this to everyone
    HandoverRequest, // can you take over this object?
    HandoverResponse, // sure I can  / no I can't
    Unknown,
}

public class ServerToServerConnection : MonoBehaviour
{

    private ClientToServerConnection clientToServerConnection;
    private NetworkDriver m_Driver;
    private NativeList<NetworkConnection> in_connections;
    private Dictionary<uint, NetworkConnection> out_connections;

    public Dictionary<uint, SerializableObject> otherObjects = new Dictionary<uint, SerializableObject>();

    public ushort getPort()
    {
        return Config.GetServer2ServerPort(this.clientToServerConnection.index);
    }
    void Start()
    {
        this.clientToServerConnection = transform.root.GetComponentInChildren<ClientToServerConnection>();
        in_connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        out_connections = new Dictionary<uint, NetworkConnection>();

        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = getPort();
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log($"Failed to bind to port {endpoint.Port}");
        else
            m_Driver.Listen();

        // connect to other known servers
        Debug.Log(Bootstrap.serverIndices.ToString());
        foreach (var index in Bootstrap.serverIndices)
        {
            if (index == this.clientToServerConnection.index) continue;
            var end = NetworkEndPoint.LoopbackIpv4;
            end.Port = Config.GetServer2ServerPort(index);
            var connection = m_Driver.Connect(end);
            out_connections.Add(index, connection);
        }

    }
    void OnDestroy()
    {
        m_Driver.Dispose();
        out_connections.Clear();
        in_connections.Dispose();
        otherObjects.Clear();
    }

    public void FixedUpdate()
    {
        m_Driver.ScheduleUpdate().Complete();
        this.removeConnections();

        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            this.in_connections.Add(c);
        }

        for (int i = 0; i < this.in_connections.Length; i++)
        {

            if (!this.in_connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(this.in_connections[i], out var reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect) {}

                if (cmd == NetworkEvent.Type.Data)
                {
                    ServerToServerMessages messageType = (ServerToServerMessages)reader.ReadByte();
                    switch (messageType)
                    {
                        case ServerToServerMessages.Ping:
                            break;


                        case ServerToServerMessages.BroadcastEvent:
                            var serializedObject = new SerializableObject();
                            serializedObject.Deserialize(ref reader);
                            this.otherObjects[serializedObject.id] = serializedObject;
                            break;


                        case ServerToServerMessages.HandoverRequest:
                            SerializableObject obj = new SerializableObject();
                            obj.Deserialize(ref reader);
                            this.clientToServerConnection.myObjects[obj.id] = Instantiate(
                                this.clientToServerConnection.objectPrefab,
                                new Vector3(obj.PositionX, obj.PositionY, obj.PositionZ),
                                Quaternion.identity);
                            this.clientToServerConnection.myObjects[obj.id].transform.parent = this.clientToServerConnection.transform;
                            this.clientToServerConnection.myObjects[obj.id].GetComponent<ObjectScript>().id = obj.id;
                            this.clientToServerConnection.myObjects[obj.id].GetComponent<ObjectScript>().manager = this.clientToServerConnection;
                            this.clientToServerConnection.myObjects[obj.id].GetComponent<Rigidbody>().velocity = new Vector3(obj.VelocityX, obj.VelocityY, obj.VelocityZ);
                            break;


                        default:
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log($"SERVER {getPort()}: disconnected from server");
                    in_connections[i].Disconnect(m_Driver);
                }
            }
        }

    }
    public void SendObjectToServer(ObjectScript o, uint id)
    {
        // Debug.Log($"SERVER {getPort()} Sending object to server {id}");
        if (!out_connections.ContainsKey(id)) return;
        if (!out_connections[id].IsCreated) return;

        m_Driver.BeginSend(NetworkPipeline.Null, out_connections[id], out var writer);
        writer.WriteByte((byte)ServerToServerMessages.HandoverRequest);
        SerializableObject.SerializeObject(o.gameObject).Serialize(ref writer);
        m_Driver.EndSend(writer);
    }

    private void removeConnections()
    {
        for (int i = 0; i < in_connections.Length; i++)
        {
            if (!in_connections[i].IsCreated)
            {
                in_connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }

}