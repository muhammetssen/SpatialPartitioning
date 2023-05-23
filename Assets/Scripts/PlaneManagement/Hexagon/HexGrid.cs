using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{

    public Color defaultColor = Color.white;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;

    HexCell[] cells;

    Canvas gridCanvas;
    HexMesh hexMesh;

    void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[Config.ParcelCount * Config.ParcelCount];

        for (int z = 0, i = 0; z < Config.ParcelCount; z++)
        {
            for (int x = 0; x < Config.ParcelCount; x++)
            {
                CreateCell(x, z, i++);
            }
        }        
    }

    void Start()
    {
        hexMesh.Triangulate(cells);

        GameObject walls = new GameObject("Walls");
        HexagonWallManager wallManager = gameObject.AddComponent<HexagonWallManager>();
        wallManager.transform.parent = walls.transform;
    }

    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * Config.ParcelCount + coordinates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.Triangulate(cells);
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
    }
}