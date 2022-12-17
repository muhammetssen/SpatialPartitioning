using Unity.Collections;

public interface INetworkSerializable
{
    void Serialize(ref DataStreamWriter writer);
    void Deserialize(ref DataStreamReader reader);
}
