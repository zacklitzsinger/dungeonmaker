using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour {

    Material lineMat;

    List<LineInfo> lines = new List<LineInfo>();

    struct LineInfo
    {
        public Vector2[] points;
        public Color color;
    }

    void Awake()
    {
        lineMat = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
    }

    void OnPostRender()
    {
        GL.Begin(GL.LINES);
        lineMat.SetPass(0);
        
        foreach(LineInfo info in lines)
        {
            GL.Color(info.color);
            foreach(Vector2 point in info.points)
                GL.Vertex(point);
        }
        GL.End();
        lines.Clear();
    }

    public void DrawLine(Vector2[] points, Color c)
    {
        lines.Add(new LineInfo() { points = points, color = c });
    }

    public void DrawArrow(Vector2 from, Vector2 to, Color c)
    {
        Vector2 arrow1 = (Quaternion.AngleAxis(30, Vector3.forward) * (from - to)).normalized * 0.5f;
        Vector2 arrow2 = (Quaternion.AngleAxis(-30, Vector3.forward) * (from - to)).normalized * 0.5f;
        lines.Add(new LineInfo() { points = new Vector2[] { from, to }, color = c });
        lines.Add(new LineInfo() { points = new Vector2[] { to, to + arrow1 }, color = c });
        lines.Add(new LineInfo() { points = new Vector2[] { to, to + arrow2 }, color = c });
    }

}
