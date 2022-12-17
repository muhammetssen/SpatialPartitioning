using Unity.Collections;

public struct NetworkBallData : INetworkSerializable
{
    public uint ID;

    public float PositionX;
    public float PositionY;
    public float PositionZ;

    public float RotationX;
    public float RotationY;
    public float RotationZ;
    public float RotationW;

    public float VelocityX;
    public float VelocityY;
    public float VelocityZ;

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUInt(ID);

        writer.WriteFloat(PositionX);
        writer.WriteFloat(PositionY);
        writer.WriteFloat(PositionZ);

        writer.WriteFloat(RotationX);
        writer.WriteFloat(RotationY);
        writer.WriteFloat(RotationZ);
        writer.WriteFloat(RotationW);

        writer.WriteFloat(VelocityX);
        writer.WriteFloat(VelocityY);
        writer.WriteFloat(VelocityZ);
    }

    public void Deserialize(ref DataStreamReader reader)
    {
        ID = reader.ReadUInt();

        PositionX = reader.ReadFloat();
        PositionY = reader.ReadFloat();
        PositionZ = reader.ReadFloat();

        RotationX = reader.ReadFloat();
        RotationY = reader.ReadFloat();
        RotationZ = reader.ReadFloat();
        RotationW = reader.ReadFloat();

        VelocityX = reader.ReadFloat();
        VelocityY = reader.ReadFloat();
        VelocityZ = reader.ReadFloat();
    }
}
