using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Networking.Transport;


public class BootStrap : MonoBehaviour
{


    public static int serverIndex = -1;
    public static int clientIndex = -1;
    public static List<uint> serverIndices = new List<uint>{0,1,2,3};

    public static bool isServer {
        get { return serverIndex != -1; }
    }
    void Awake()
    {
        //TODO parse command line arguments
    }
}
