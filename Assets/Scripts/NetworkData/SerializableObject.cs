using Unity.Collections;
using UnityEngine;

public struct SerializableObject : ISerializedObject
{

    public uint id;


    public float PositionX;
    public float PositionY;
    public float PositionZ;

    public float VelocityX;
    public float VelocityY;
    public float VelocityZ;

    public bool IsBroadcast;
    public void Serialize(ref Unity.Collections.DataStreamWriter writer)
    {
        writer.WriteUInt(id);

        writer.WriteFloat(PositionX);
        writer.WriteFloat(PositionY);
        writer.WriteFloat(PositionZ);

        writer.WriteFloat(VelocityX);
        writer.WriteFloat(VelocityY);
        writer.WriteFloat(VelocityZ);

        writer.WriteByte((byte)(IsBroadcast ? 1 : 0));
    }
    public void Deserialize(ref Unity.Collections.DataStreamReader reader)
    {
        id = reader.ReadUInt();

        PositionX = reader.ReadFloat();
        PositionY = reader.ReadFloat();
        PositionZ = reader.ReadFloat();

        VelocityX = reader.ReadFloat();
        VelocityY = reader.ReadFloat();
        VelocityZ = reader.ReadFloat();

        IsBroadcast = reader.ReadByte() == 1;

    }
    public static SerializableObject SerializeObject(GameObject obj)
    {
        SerializableObject serializedObject = new SerializableObject();
        serializedObject.id = obj.GetComponent<ObjectScript>().id;
        serializedObject.PositionX = obj.transform.position.x;
        serializedObject.PositionY = obj.transform.position.y;
        serializedObject.PositionZ = obj.transform.position.z;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        serializedObject.VelocityX = rb.velocity.x;
        serializedObject.VelocityY = rb.velocity.y;
        serializedObject.VelocityZ = rb.velocity.z;

        serializedObject.IsBroadcast = obj.GetComponent<ObjectScript>().IsBroadcast;
        if(serializedObject.IsBroadcast)
        {
            // set color to red
            obj.GetComponent<Renderer>().material.color = Color.red;
        }

        return serializedObject;
    }
}