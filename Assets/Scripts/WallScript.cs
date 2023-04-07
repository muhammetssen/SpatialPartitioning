using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    [SerializeField]
    public GameObject wallPrefab;
    void Start()
    {
        for (int row = 0; row < Config.ParcelCount; row++)
        {
            // Create the walls on the left and right side of the grid
            var wall = Instantiate(this.wallPrefab, new Vector3(row * Config.ParcelSize, 0, -Config.ParcelSize / 2), Quaternion.identity);
            wall.transform.Rotate(0, 90, 0);
            wall = Instantiate(this.wallPrefab, new Vector3(row * Config.ParcelSize, 0, Config.ParcelSize * Config.ParcelCount - Config.ParcelSize / 2), Quaternion.identity);
            wall.transform.Rotate(0, 90, 0);
        }
        
        for (int column = 0; column < Config.ParcelCount; column++)
        {
            // Create the walls on the left and right side of the grid
            var wall = Instantiate(this.wallPrefab, new Vector3(-Config.ParcelSize / 2, 0, column * Config.ParcelSize ), Quaternion.identity);
            wall.transform.Rotate(0, 0, 0);
            wall = Instantiate(this.wallPrefab, new Vector3(Config.ParcelSize * Config.ParcelCount - Config.ParcelSize / 2, 0, column * Config.ParcelSize ), Quaternion.identity);
            wall.transform.Rotate(0, 0, 0);
        }
    }

}
