using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    [SerializeField]
    public GameObject wallPrefab;
    void CreateWall(Vector3 position, bool isVertical){
        var wall = Instantiate(this.wallPrefab, position, Quaternion.identity);
        wall.transform.Rotate(0, isVertical ? 90 : 0, 0);
        wall.transform.localScale = new Vector3(0.1f, 3, Config.ParcelSize);
        wall.transform.parent = this.transform;
    }
    
    void Start()
    {
        for (int row = 0; row < Config.ParcelCount; row++)
        {
            // Create walls on the left and right sides of the grid
            this.CreateWall(new Vector3(row * Config.ParcelSize, 0, -Config.ParcelSize / 2), true);
            this.CreateWall(new Vector3(row * Config.ParcelSize, 0, Config.ParcelSize * Config.ParcelCount - Config.ParcelSize / 2), true);
        }

        for (int column = 0; column < Config.ParcelCount; column++)
        {
            // Create walls on the top and bottom sides of the grid
            this.CreateWall(new Vector3(-Config.ParcelSize / 2, 0, column * Config.ParcelSize), false);
            this.CreateWall(new Vector3(Config.ParcelSize * Config.ParcelCount - Config.ParcelSize / 2, 0, column * Config.ParcelSize), false);
        }
    }

}
