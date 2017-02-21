using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour {

    public Material lineMat;

    List<LineInfo> lines = new List<LineInfo>();

    struct LineInfo
    {
        public Vector2[] points;
        public Color color;
    }

    void OnPostRender()
    {
        GL.PushMatrix();
        lineMat.SetPass(0);
        GL.Begin(GL.LINES);
        
        foreach(LineInfo info in lines)
        {
            GL.Color(info.color);
            foreach(Vector2 point in info.points)
                GL.Vertex(point);
        }
        GL.End();
        GL.PopMatrix();
        lines.Clear();
    }

    public void DrawLine(Vector2[] points, Color c)
    {
        lines.Add(new LineInfo() { points = points, color = c });
    }

    public void DrawArrow(Vector2 from, Vector2 to, Color c)
    {
        Vector2 arrow1 = (Quaternion.AngleAxis(30, Vector3.forward) * (from - to)).normalized * 0.3f;
        Vector2 arrow2 = (Quaternion.AngleAxis(-30, Vector3.forward) * (from - to)).normalized * 0.3f;
        lines.Add(new LineInfo() { points = new Vector2[] { from, to }, color = c });
        lines.Add(new LineInfo() { points = new Vector2[] { to, to + arrow1 }, color = c });
        lines.Add(new LineInfo() { points = new Vector2[] { to, to + arrow2 }, color = c });
    }

}
