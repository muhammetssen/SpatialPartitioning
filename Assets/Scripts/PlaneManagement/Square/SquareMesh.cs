using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SquareMesh : MonoBehaviour
{
    public int row;
    public int column;

    [SerializeField]
    private TMP_Text textMesh;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

    }
    public void updateText()
    {
        textMesh.text = this.row + "-" + this.column;
    }

}
