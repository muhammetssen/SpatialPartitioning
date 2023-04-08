using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class CreatePlanes : MonoBehaviour
{
    public GameObject plane;
    public static List<PlaneScript> planes = new List<PlaneScript>();
    // Start is called before the first frame update
    private NetworkDriver m1_Driver;
    void Start()
    {
         this.m1_Driver = NetworkDriver.Create();
        NetworkConnection connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 12347;
        connection = m1_Driver.Connect(endpoint);
        

        for (int row = 0; row < Config.ParcelCount; row++)
        {
            for (int column = 0; column < Config.ParcelCount; column++)
            {
                var plane = Instantiate(this.plane, new Vector3(row * Config.ParcelSize, 0, column * Config.ParcelSize), Quaternion.identity);
                plane.transform.localScale = new Vector3(Config.ParcelSize / 10, 1, Config.ParcelSize / 10);
                plane.transform.parent = this.transform;

                PlaneScript planeScript = plane.GetComponent<PlaneScript>();
                planeScript.row = row;
                planeScript.column = column;
                planeScript.updateText();
                planes.Add(planeScript);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy()
    {
        planes.Clear();
        m1_Driver.Dispose();

    }
}
