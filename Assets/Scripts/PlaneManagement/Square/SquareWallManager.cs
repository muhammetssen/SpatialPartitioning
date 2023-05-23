using UnityEngine;

class SquareWallManager : WallManager
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void InitiateWalls()
    {
       const float size = Config.ParcelSize;
        int count = Config.ParcelCount;
        for (int row = 0; row < count; row++)
        {
            // Create walls on the left and right sides of the grid
            this.CreateWall(new Vector3(row * size, 0, -size / 2), true);
            this.CreateWall(new Vector3(row * size, 0, size * count - size / 2), true);
        }

        for (int column = 0; column < count; column++)
        {
            // Create walls on the top and bottom sides of the grid
            this.CreateWall(new Vector3(-size / 2, 0, column * size), false);
            this.CreateWall(new Vector3(size * count - size / 2, 0, column * size), false);
        }
       
    }
    protected override void CreateWall(Vector3 position, bool isVertical){
        var wall = Instantiate(this.wallPrefab, position, Quaternion.identity);
        wall.transform.Rotate(0, isVertical ? 90 : 0, 0);
        wall.transform.localScale = new Vector3(0.1f, 3, Config.ParcelSize);
        wall.transform.parent = this.transform;
    }
}