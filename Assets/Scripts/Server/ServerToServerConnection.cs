using System;
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
    InBuffer,
    Unknown,
}

public class ServerToServerConnection : MonoBehaviour
{
    private const float BUFFER_SIZE = (Config.ParcelSize / 2) * 1.4f; // 1.4 is ~sqrt(2)
    [SerializeField]
    private ClientToServerConnection clientToServerConnection;
    private NetworkDriver m_Driver;
    private NativeList<NetworkConnection> in_connections;
    private Dictionary<uint, NetworkConnection> out_connections;

    public Dictionary<uint, SerializableObject> otherObjects = new Dictionary<uint, SerializableObject>();

    public ushort getPort()
    {
        return Config.GetServer2ServerPort(this.clientToServerConnection.index);
    }
    void DrawCircle(LineRenderer renderer, int steps, float radius){
        renderer.positionCount = steps + 1;
        renderer.useWorldSpace = false;
        renderer.startWidth = 0.2f;
        renderer.endWidth = 0.2f;
        for (int i = 0; i < steps + 1; i++)
        {
            float angle = i * Mathf.PI * 2 / steps;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            renderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }
    void Start()
    {
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
        StartCoroutine(BufferCoroutine());
        StartCoroutine(ClearBufferArea());
        DrawCircle(this.GetComponent<LineRenderer>(), 50, BUFFER_SIZE);

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
                            if(this.otherObjects.ContainsKey(obj.id))
                            {
                                this.otherObjects.Remove(obj.id);
                            }
                            break;
                        
                        case ServerToServerMessages.InBuffer:
                            var sharedObject = new SerializableObject();
                            sharedObject.Deserialize(ref reader);
                            this.otherObjects[sharedObject.id] = sharedObject;
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
        public void AlertServerBuffer(ObjectScript o, uint id)
    {
        if (!out_connections.ContainsKey(id)) return;
        if (!out_connections[id].IsCreated) return;

        m_Driver.BeginSend(NetworkPipeline.Null, out_connections[id], out var writer);
        writer.WriteByte((byte)ServerToServerMessages.InBuffer);
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
    private IEnumerator BufferCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Config.UpdateInterval);
            foreach (var o in this.clientToServerConnection.myObjects)
            {
                foreach (var id in Bootstrap.serverIndices)
                {
                    if (id == this.clientToServerConnection.index) continue;
                    var serverCenter = Config.GetServerCenter(id);
                    var objectCenter = new Tuple<float, float>(o.Value.transform.position.x, o.Value.transform.position.z);
                    if (!checkBufferOverlap(serverCenter, objectCenter)) continue;
                    AlertServerBuffer(o.Value.GetComponent<ObjectScript>(), id);
                }
            }
        }
    }
    private bool checkBufferOverlap(Tuple<float, float> server,Tuple<float, float> other, float radius = BUFFER_SIZE){
        return Math.Sqrt(Math.Pow(server.Item1 - other.Item1, 2) + Math.Pow(server.Item2 - other.Item2, 2)) < radius;
    }

    private IEnumerator ClearBufferArea(){
        while (true)
        {
            
            yield return new WaitForSeconds(Config.UpdateInterval * 10);
            // foreach (var o in this.otherObjects)
            // {
            //     bool remove = true; // will be changed with last updated
            //     if (!remove) continue;
            //     this.otherObjects.Remove(o.Key);
            // }
            this.otherObjects.Clear();
        }



    }

}