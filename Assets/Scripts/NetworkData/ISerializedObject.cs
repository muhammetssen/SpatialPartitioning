using Unity.Collections;
using Unity.Networking.Transport;

public interface ISerializedObject
{
    void Serialize(ref DataStreamWriter writer);
    void Deserialize(ref DataStreamReader reader);
}
