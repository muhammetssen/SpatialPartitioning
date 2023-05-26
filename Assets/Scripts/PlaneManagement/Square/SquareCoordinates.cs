using UnityEngine;

[System.Serializable]
public struct SquareCoordinates {
    private int x, z;

	public int X {
		get {
			return x;
		}
	}

	public int Z {
		get {
			return z;
		}
	}
    public SquareCoordinates (int x, int z) {
		this.x = x;
		this.z = z;
	}
	public static SquareCoordinates FromPosition (Vector3 position) {
        int currentRow = (int)(position.x + Config.ParcelSize / 2) / Config.ParcelSize;
        int currentCol = (int)(position.z + Config.ParcelSize / 2) / Config.ParcelSize;
        return new SquareCoordinates(currentRow, currentCol);
	}

	public int ParcelIndex {
		get {
			return Z * Config.ParcelCount + X;
		}
	}
}