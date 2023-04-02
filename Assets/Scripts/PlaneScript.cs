using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// function that returns a random color from a list of colors
public static class ColorExtensions
{
    public static List<Color> colors = new List<Color>
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.white,
            Color.black,
            Color.gray
        };
}

public class PlaneScript : MonoBehaviour
{
    public int row;
    public int column;

    [SerializeField]
    private TMP_Text textMesh;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        // gameObject.GetComponent<Renderer>().material.color = ColorExtensions.colors[3*row + column];

    }
    public void updateText()
    {
        textMesh.text = this.row + "-" + this.column;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
