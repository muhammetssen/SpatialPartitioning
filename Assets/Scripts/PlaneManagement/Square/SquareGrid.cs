using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class SquareGrid : MonoBehaviour
{
    public GameObject plane;
    public static List<SquareMesh> cells = new List<SquareMesh>();
    void Start()
    {
        GameObject meshes = new GameObject("Meshes");
        for (int row = 0; row < Config.ParcelCount; row++)
        {
            for (int column = 0; column < Config.ParcelCount; column++)
            {
                var plane = Instantiate(this.plane, new Vector3(row * Config.ParcelSize, 0, column * Config.ParcelSize), Quaternion.identity);
                plane.transform.localScale = new Vector3(Config.ParcelSize / 10, 1, Config.ParcelSize / 10);
                plane.transform.parent = meshes.transform;

                SquareMesh mesh = plane.GetComponent<SquareMesh>();
                mesh.row = row;
                mesh.column = column;
                mesh.updateText();
                cells.Add(mesh);
            }
        }
        GameObject walls = new GameObject("Walls");
        SquareWallManager wallManager = gameObject.AddComponent<SquareWallManager>();
        wallManager.transform.parent = walls.transform;

    }

    void OnDestroy()
    {
        cells.Clear();

    }
}
