using System;
using UnityEngine;

public class Config : MonoBehaviour
{
    public const int ParcelSize = 50;
    public static int ParcelCount = 2;

    public static int ObjectCount = 10;
    public static string ServerIP = "3.122.49.197";

    public static SolutionTypes SolutionType = SolutionTypes.ServerBuffering;
    public static float AOIRadius = 20f;

    public static float UpdateInterval = 0.01f;

    public static bool SingleInstance = false;

    public static PlaneType planeType = PlaneType.Hexagon;

    public static uint GetParcelId(Vector3 position)
    {
        if (Config.planeType == Config.PlaneType.Hexagon)
            return (uint)HexCoordinates.FromPosition(position).ParcelIndex;
        else if (Config.planeType == Config.PlaneType.Square)
            return (uint)SquareCoordinates.FromPosition(position).ParcelIndex;
        else
            throw new Exception("Invalid plane type");
    }

    public static ushort GetClient2ServerPort(uint serverIndex)
    {
        var p = (ushort)(10000 + serverIndex * 2);
#if UNITY_WEBGL
            return (ushort)(p + 1);
#else
        return p;
#endif
    }

    public static ushort GetServer2ServerPort(uint serverIndex)
    {
        return (ushort)(20000 + serverIndex);
    }

    public static Tuple<float, float> GetServerCenter(uint serverIndex)
    {
        if (Config.planeType == Config.PlaneType.Hexagon)
        {
            var coors = HexCoordinates.FromParcelIndex((int)serverIndex);
            return new Tuple<float, float>(coors.X, coors.Z);
        }
        else if (Config.planeType == Config.PlaneType.Square)
        {
            var coors = SquareCoordinates.FromParcelIndex((int)serverIndex);
            return new Tuple<float, float>(coors.X, coors.Z);
        }
        else
            throw new Exception("Invalid plane type");

    }

    public static float GetBufferSize()
    {
        if (Config.planeType == Config.PlaneType.Hexagon)
            return Config.ParcelSize * 1.1f;
        else if (Config.planeType == Config.PlaneType.Square)
            return Config.ParcelSize * 0.9f;
        else
            throw new Exception("Invalid plane type");
    }
    public enum PlaneType
    {
        Square,
        Hexagon
    }
}