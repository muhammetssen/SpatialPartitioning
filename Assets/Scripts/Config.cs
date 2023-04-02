using System;
using UnityEngine;

public class Config : MonoBehaviour
{
    public static int ParcelSize = 10;
    public static int ParcelCount = 2;

    public static Tuple<int, int> GetParcelIndex(Vector3 position)
    {
        int currentRow = (int)(position.x + Config.ParcelSize / 2) / Config.ParcelSize;
        int currentCol = (int)(position.z + Config.ParcelSize / 2) / Config.ParcelSize;
        return new Tuple<int, int>(currentRow, currentCol);
    }

    public static uint GetParcelId(Vector3 position)
    {
        var parcelIndex = GetParcelIndex(position);
        return (uint)(parcelIndex.Item1 * Config.ParcelCount + parcelIndex.Item2);
    }

    public static ushort GetClient2ServerPort(uint serverIndex){
        return (ushort)(10000 + serverIndex);
    }
    
    public static ushort GetServer2ServerPort(uint serverIndex){
        return (ushort)(20000 + serverIndex);
    }
}