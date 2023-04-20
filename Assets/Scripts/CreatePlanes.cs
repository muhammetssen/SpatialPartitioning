using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class CreatePlanes : MonoBehaviour
{
    public GameObject plane;
    public static List<PlaneScript> planes = new List<PlaneScript>();
    void Start()
    {

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

    void OnDestroy()
    {
        planes.Clear();

    }
}
