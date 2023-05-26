using UnityEngine;
using System.Collections.Generic;
public enum Corners
{
    Top,
    TopRight,
    BottomRight,
    Bottom,
    BottomLeft,
    TopLeft
};
public static class HexMetrics
{

    public const float outerRadius = Config.ParcelSize;

    public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius), // top
		new Vector3(innerRadius, 0f, 0.5f * outerRadius), // top right
		new Vector3(innerRadius, 0f, -0.5f * outerRadius), // bottom right
		new Vector3(0f, 0f, -outerRadius), // bottom
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius), // bottom left
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius), // top left
		new Vector3(0f, 0f, outerRadius) // top again to close the loop
	};

    public static Dictionary<Corners, Vector3> cornerDict = new Dictionary<Corners, Vector3> {
		{Corners.Top, 			corners[0]},
		{Corners.TopRight,		corners[1]},
		{Corners.BottomRight, 	corners[2]},
		{Corners.Bottom, 		corners[3]},
		{Corners.BottomLeft, 	corners[4]},
		{Corners.TopLeft, 		corners[5]}
	};
}