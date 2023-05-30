using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public new GameObject camera;
    private GameObject ConnectedTo;

    LineRenderer circleRenderer;
    // Start is called before the first frame update
    void Start()
    {
        this.ConnectedTo = Instantiate(Resources.Load("ConnectedTo") as GameObject);
        this.ConnectedTo.transform.parent = this.transform;
        camera = GameObject.Find("Main Camera");

        DrawCircle(transform.GetChild(0).gameObject.GetComponent<LineRenderer>(), 50, 20);
        // DrawCircle(transform.GetChild(1).gameObject.GetComponent<LineRenderer>(), 50, 6);

    }

    // Update is called once per frame
    void Update()
    {
        uint speed = 20;
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * speed, 0, Input.GetAxis("Vertical") * Time.deltaTime * speed);
        this.ConnectedTo.transform.position = new Vector3(transform.position.x, 0, transform.position.z - 1);

        // Debug.Log("Player position: " + transform.position);
        camera.transform.position = new Vector3(transform.position.x, 140, transform.position.z);

    }

    void DrawCircle(LineRenderer renderer, int steps, float radius)
    {
        renderer.positionCount = steps + 1;
        renderer.useWorldSpace = false;
        renderer.startWidth = 0.2f;
        renderer.endWidth = 0.2f;
        for (int i = 0; i < steps + 1; i++)
        {
            float angle = i * Mathf.PI * 2 / steps;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            renderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}
