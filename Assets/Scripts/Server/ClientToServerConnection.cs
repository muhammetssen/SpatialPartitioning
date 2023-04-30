using Unity.Networking.Transport;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


public class PlayerClass
{
    public uint id { get; set; }
    public Vector3 position { get; set; }
    public PlayerClass(uint id, Vector3 position)
    {
        this.id = id;
        this.position = position;
    }
}
public enum ClientToServerMessages : byte
{
    Ping, // hello, are you there?
    RegisterMe,

}

public class ClientToServerConnection : MonoBehaviour
{
    [SerializeField]
    public uint index = 0;
    public GameObject objectPrefab;
    public Dictionary<uint, GameObject> myObjects;
    private ushort port;
    [SerializeField]
    public Vector3 planeCoors;
    private Dictionary<uint, PlayerClass> players;
    private NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_connections;
    public ServerToServerConnection serverToServerConnection;
    void Start()
    {
        this.port = Config.GetClient2ServerPort(this.index);
        this.setPlaneCoors();


        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;

        endpoint.Port = (ushort)this.port;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log($"Failed to bind to port {this.port}");
        else
        {
            m_Driver.Listen();
            Debug.Log($"SERVER-{this.port}: Listening on port {this.port}");
        }

        m_connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        players = new Dictionary<uint, PlayerClass>();

        myObjects = new Dictionary<uint, GameObject>();
        Debug.Log($"SERVER-{this.port}: Ready to accept connections on port {this.port}");
        StartCoroutine(waitInit());

    }

    private IEnumerator<WaitForSeconds> waitInit()
    {
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < Config.ObjectCount; i++)
        {

            uint id = getRandom();
            myObjects[id] = Instantiate(objectPrefab, transform.position, Quaternion.identity);
            myObjects[id].GetComponent<ObjectScript>().manager = this;
            myObjects[id].GetComponent<ObjectScript>().id = id;
            myObjects[id].GetComponent<Rigidbody>().velocity = Vector3.left * 12f + Vector3.forward * 24f;
        }
    }


    private uint getRandom()
    {
        return (uint)Random.Range(1, 1000000);
    }
    void FixedUpdate()
    {
        m_Driver.ScheduleUpdate().Complete();
        this.removeConnections();

        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            this.m_connections.Add(c);
        }

        for (int i = 0; i < this.m_connections.Length; i++)
        {
            if (!this.m_connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(this.m_connections[i], out var reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    ClientToServerMessages messageType = (ClientToServerMessages)reader.ReadByte();
                    if (!m_connections[i].IsCreated) continue;
                    switch (messageType)
                    {
                        case ClientToServerMessages.Ping:
                            uint playerId = reader.ReadUInt();
                            if (!players.ContainsKey(playerId))
                            {
                                Debug.Log($"SERVER-{this.port}: Unknown player id: {playerId}");
                                break;
                            }
                            players[playerId].position = new Vector3(
                                reader.ReadFloat(),
                                reader.ReadFloat(),
                                reader.ReadFloat()
                            );

                            foreach (var k in myObjects.Keys)
                            {
                                m_Driver.BeginSend(NetworkPipeline.Null, m_connections[i], out var writer);
                                writer.WriteByte((byte)ServerToClientMessages.ObjectUpdate);
                                SerializableObject.SerializeObject(myObjects[k]).Serialize(ref writer);
                                m_Driver.EndSend(writer);
                            }

                            foreach (var k in serverToServerConnection.otherObjects.Keys)
                            {
                                m_Driver.BeginSend(NetworkPipeline.Null, m_connections[i], out var writer);
                                writer.WriteByte((byte)ServerToClientMessages.TemporaryObjectUpdate);
                                serverToServerConnection.otherObjects[k].Serialize(ref writer);
                                m_Driver.EndSend(writer);
                            }

                            // check if client is out of my bounds
                            uint expectedServerIndex = Config.GetParcelId(players[playerId].position);
                            // Debug.Log($"SERVER-{this.port}: expectedServerIndex: {expectedServerIndex}");
                            if (expectedServerIndex != this.index)
                            {
                                // send the client to the correct server
                                m_Driver.BeginSend(NetworkPipeline.Null, m_connections[i], out var writer4);
                                writer4.WriteByte((byte)ServerToClientMessages.ServerChange);
                                writer4.WriteUInt(expectedServerIndex);
                                m_Driver.EndSend(writer4);
                                // m_connections[i].Disconnect(m_Driver);
                            }
                            break;



                        case ClientToServerMessages.RegisterMe:
                            m_Driver.BeginSend(NetworkPipeline.Null, m_connections[i], out var writer3);
                            writer3.WriteByte((byte)ServerToClientMessages.Welcome);
                            uint id = getRandom();
                            players.Add(id, new PlayerClass
                            (id,
                              new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat())
                            ));
                            writer3.WriteUInt(id);
                            m_Driver.EndSend(writer3);
                            break;



                        default:
                            Debug.Log($"SERVER-{this.port}: Unknown message from client");
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_connections[i].Disconnect(m_Driver);
                }
            }
        }

    }

    void OnDestroy()
    {
        m_Driver.Dispose();
        m_connections.Dispose();
    }

    private void removeConnections()
    {
        for (int i = 0; i < m_connections.Length; i++)
        {
            if (!m_connections[i].IsCreated)
            {
                m_connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void setPlaneCoors()
    {
        int r = (int)(index / Config.ParcelCount);
        int c = (int)(index % Config.ParcelCount);

        this.planeCoors = new Vector3(
            r * Config.ParcelSize,
            0,
            c * Config.ParcelSize
        );
        transform.position = this.planeCoors;

    }

    public void alertClientsOfObjectRemoval(uint id)
    {
        for (int i = 0; i < this.m_connections.Length; i++)
        {
            if (!this.m_connections[i].IsCreated) continue;
            m_Driver.BeginSend(NetworkPipeline.Null, m_connections[i], out var writer);
            writer.WriteByte((byte)ServerToClientMessages.ObjectRemoval);
            writer.WriteUInt(id);
            m_Driver.EndSend(writer);
        }
    }
}