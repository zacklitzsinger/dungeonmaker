using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour {

    Material lineMat;

    List <Vector2> points = new List<Vector2>();
    Color color;

    void Awake()
    {
        lineMat = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
    }

    void OnPostRender()
    {
        GL.Begin(GL.LINES);
        lineMat.SetPass(0);
        GL.Color(color);
        foreach(Vector2 point in points)
        {
            GL.Vertex(point);
        }
        GL.End();
        points.Clear();
    }

    public void DrawLine(List<Vector2> points, Color c)
    {
        this.points = points;
        this.color = c;
    }

}
