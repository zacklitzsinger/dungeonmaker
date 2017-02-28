using System.Collections.Generic;
using UnityEngine;

public class NavMap
{
    Dictionary<Vector2, List<ObjectData>> map;
    Rect bounds = new Rect();

    public Rect Bounds { get { return bounds; } }

    public NavMap(Dictionary<Vector2, List<ObjectData>> map)
    {
        this.map = map;
    }

    public void RecalculateBounds()
    {
        bounds = new Rect();
        foreach (Vector2 point in map.Keys)
        {
            if (point.x < bounds.xMin)
                bounds.xMin = point.x - 2;
            else if (point.x > bounds.xMax)
                bounds.xMax = point.x + 2;
            if (point.y < bounds.yMin)
                bounds.yMin = point.y - 2;
            else if (point.y > bounds.yMax)
                bounds.yMax = point.y + 2;
        }
    }

    public bool Passable(Vector2 node)
    {
        if (!map.ContainsKey(node))
            return false;
        List<ObjectData> goList = map[node];
        foreach (ObjectData info in goList)
            if (info != null && info.type == ObjectType.Wall)
                return false;
        return true;
    }

    public bool SeeThrough(Vector2 node)
    {
        if (!map.ContainsKey(node))
            return true;
        List<ObjectData> goList = map[node];
        foreach (ObjectData info in goList)
            if (info != null && !info.seeThrough)
                return false;
        return true;
    }

    public float DistanceBetween(Vector2 from, Vector2 to)
    {
        return Vector2.Distance(from, to);
    }

    public List<Vector2> GetPotentialNeighbors(Vector2 current)
    {
        List<Vector2> ret = new List<Vector2>();
        for (int x = (int)current.x - 1; x <= current.x + 1; x++)
            for (int y = (int)current.y - 1; y <= current.y + 1; y++)
            {
                if (!bounds.Contains(new Vector2(x, y)) || x == current.x && y == current.y)
                    continue;
                ret.Add(new Vector2(x, y));
            }
        return ret;
    }

    public List<Vector2> GetNeighbors(Vector2 current, bool includeEmpty = true, bool ignoreSeeThrough = false)
    {
        return GetPotentialNeighbors(current).FindAll((n) => {
            return (includeEmpty && !map.ContainsKey(n) || (ignoreSeeThrough && SeeThrough(n)) ||  Passable(n));
        });
    }
}
