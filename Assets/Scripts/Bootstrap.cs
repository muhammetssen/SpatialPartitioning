using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Networking.Transport;


public class Bootstrap : MonoBehaviour
{


    public static int serverIndex = -1;
    public static int serverCount = 4;
    public static List<uint> serverIndices = new List<uint> { 0, 1, 2, 3 };

    public static bool isServer
    {
        get { return serverIndex != -1; }
    }
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Random.InitState(0); // Make sure that the random numbers are the same for all instances to make the results reproducible
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-serverIndex")
            {
                if (int.TryParse(args[i + 1], out int ServerIndex))
                {
                    serverIndex = ServerIndex;
                    // Debug.Log("Server index: " + serverIndex);
                }
            }
            if (args[i] == "-parcelCount")
            {
                if (int.TryParse(args[i + 1], out int ParcelCount)) Config.ParcelCount = ParcelCount;
            }
            if (args[i] == "-objectCount")
            {
                if (int.TryParse(args[i + 1], out int ObjectCount)) Config.ObjectCount = ObjectCount;
            }
            if (args[i] == "-solutionType")
            {
                if (int.TryParse(args[i + 1], out int SolutionType)) Config.SolutionType = (SolutionTypes)SolutionType;
            }
            if (args[i] == "-parcelType")
            {
                string planeType =  args[i + 1].ToLower();
                if (planeType == "square") Config.planeType = Config.PlaneType.Square;
                else if (planeType == "hexagon") Config.planeType = Config.PlaneType.Hexagon;
                Debug.Log("Plane type: " + Config.planeType.ToString());
            }
            if (args[i] == "-AOIRadius")
            {
                if (float.TryParse(args[i + 1], out float AOIBuffer)) Config.AOIRadius = AOIBuffer;
            }


        }

        serverCount = Config.ParcelCount * Config.ParcelCount;
        serverIndices = new List<uint>();
        for (int j = 0; j < serverCount; j++) serverIndices.Add((uint)j);

    }
    void Start()
    {
        InitializeGrid();
        Debug.Log($"Server index: {serverIndex}, server count: {serverCount}, is server: {isServer}, solution type: {Config.SolutionType}");
        if (Config.SingleInstance)
        {

#if UNITY_EDITOR
            for (int i = 0; i < serverCount; i++)
            {
                Debug.Log("Creating server with index: " + i);
                var serverObject = Instantiate(Resources.Load("Server") as GameObject);
                serverObject.GetComponent<ClientToServerConnection>().index = (uint)i;
                serverObject.transform.parent = transform.root;
            }
            System.Threading.Thread.Sleep(1000);
#else
            Debug.Log("Running in build");
#endif
            var playerObject = Instantiate(Resources.Load("Player") as GameObject);
            playerObject.transform.parent = transform.root;
        }
        else
        {
            if (isServer)
            {
                var serverObject = Instantiate(Resources.Load("Server") as GameObject);
                serverObject.GetComponent<ClientToServerConnection>().index = (uint)serverIndex;
            }
            else
            {
                var playerObject = Instantiate(Resources.Load("Player") as GameObject);
                Debug.Log("Player object: " + playerObject);
            }
        }
    }
    void InitializeGrid()
    {
        switch (Config.planeType)
        {
            case Config.PlaneType.Square:
                var squareGrid = Instantiate(Resources.Load("SquareGrid") as GameObject);
                break;
            case Config.PlaneType.Hexagon:
                var hexagonGrid = Instantiate(Resources.Load("HexGrid") as GameObject);
                break;
        }

    }
}
