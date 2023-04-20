using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public new GameObject camera;
    private GameObject ConnectedTo;
    // Start is called before the first frame update
    void Start()
    {
        this.ConnectedTo = Instantiate(Resources.Load("ConnectedTo") as GameObject);
        this.ConnectedTo.transform.parent = this.transform;
        camera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * 10, 0, Input.GetAxis("Vertical") * Time.deltaTime * 10);
        this.ConnectedTo.transform.position = new Vector3(transform.position.x, 0, transform.position.z - 1);

        // Debug.Log("Player position: " + transform.position);
        camera.transform.position = new Vector3(transform.position.x, 80, transform.position.z);

        int currentRow = (int)(transform.position.x + Config.ParcelSize / 2) / Config.ParcelSize;
        int currentCol = (int)(transform.position.z + Config.ParcelSize / 2) / Config.ParcelSize;
        // Debug.Log("Player is in parcel: " + currentRow + " " + currentCol);
    }
}
