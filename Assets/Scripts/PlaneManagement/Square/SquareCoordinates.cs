using UnityEngine;

[System.Serializable]
public class SquareCoordinates {
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

		public static SquareCoordinates FromParcelIndex (int index) {
		int z = index / Config.ParcelCount;
		int x = index - z * Config.ParcelCount;
		return new SquareCoordinates(x, z);
	}

	public Vector3 position {
		get {
			return new Vector3(
				X * Config.ParcelSize,
				0,
				Z * Config.ParcelSize
			);
		}
	}
}