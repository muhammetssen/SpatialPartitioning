using UnityEngine;

class HexagonWallManager : WallManager
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void InitiateWalls()
    {
        const float size = Config.ParcelSize;
        int count = Config.ParcelCount;
        float sqrt3 = Mathf.Sqrt(3);
        float innerSize = size * sqrt3 / 2;

        // bottom
        for (int row = 0; row < count; row++)
        {
            float x = -size * sqrt3 / 4;
            float z = x * sqrt3;
            this.CreateWall(new Vector3(x + row * size * sqrt3, 0, z), -60);
            this.CreateWall(new Vector3(-1 * x + row * size * sqrt3, 0, z), 60);
        }

        // top
        float x_offset = 0;
        if (count % 2 == 1) x_offset = -innerSize;

        for (int row = 0; row < count; row++)
        {
            float x = x_offset + size * sqrt3 / 4;
            float z = (float)(size * ((count - 1) * 3.0 / 2 + 3.0 / 4));
            this.CreateWall(new Vector3(x + row * size * sqrt3, 0, z), 60);
            this.CreateWall(new Vector3(x + row * size * sqrt3 + innerSize, 0, z), -60);
        }

        for (int row = 0; row < count; row++)
        {
            if (row % 2 == 0)
            {
                float x = -innerSize;
                float z = 0;
                this.CreateWall(new Vector3(x, 0, z + row * size * 3 / 2), 0); // left
                this.CreateWall(new Vector3(x + count * size * sqrt3, 0, z + row * size * 3 / 2), 0); // right

                this.CreateWall(new Vector3(x + innerSize / 2, 0, (float)(z + row * size * 3 / 2 + 0.75 * size)), 60); // left cross
                this.CreateWall(new Vector3(x + innerSize / 2 + count * size * sqrt3, 0, (float)(z + row * size * 3 / 2 + 0.75 * size)), 60); // right cross

            }
            else
            {
                float x = 0;
                float z = 0;
                this.CreateWall(new Vector3(x, 0, z + row * size * 3 / 2), 0); // left
                this.CreateWall(new Vector3(x + count * size * sqrt3, 0, z + row * size * 3 / 2), 0); // right

                this.CreateWall(new Vector3(x - innerSize / 2, 0, (float)(z + row * size * 3 / 2 + 0.75 * size)), -60); // left cross
                this.CreateWall(new Vector3(x - innerSize / 2 + count * size * sqrt3, 0, (float)(z + row * size * 3 / 2 + 0.75 * size)), -60); // right cross
            }
        }
    }
    protected override void CreateWall(Vector3 position, float angle)
    {
        var wall = Instantiate(this.wallPrefab, position, Quaternion.identity);
        wall.transform.Rotate(0, angle, 0);
        wall.transform.localScale = new Vector3(0.1f, 3, Config.ParcelSize);
        wall.transform.parent = this.transform;
    }
}