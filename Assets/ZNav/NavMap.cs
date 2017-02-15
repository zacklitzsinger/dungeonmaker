using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNode : NavNode<MapNode>
{
    public int x, y;

    public MapNode(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public MapNode(Vector2 v) : this((int)v.x, (int)v.y) { }

    public override bool Equals(object obj)
    {
        MapNode other = obj as MapNode;
        if (other == null)
            return false;
        return (other.x == x && other.y == y);
    }

    public override int GetHashCode()
    {
        // Prime numbers to avoid collisions
        int hash = 13;
        hash += 7 * x.GetHashCode();
        hash += 11 * y.GetHashCode();
        return hash;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public override string ToString()
    {
        return "<MapNode x: " + x + ", y: " + y + ">";
    }
}

public class NavMap : INavMap<MapNode>
{
    Dictionary<Vector2, List<GameObject>> map;
    Dictionary<Vector2, MapNode> hackNodeMap = new Dictionary<Vector2, MapNode>();
    Rect bounds = new Rect();

    public NavMap(Dictionary<Vector2, List<GameObject>> map)
    {
        this.map = map;
    }

    public MapNode GetNode(int x, int y)
    {
        MapNode node;
        if (!hackNodeMap.TryGetValue(new Vector2(x, y), out node))
            node = hackNodeMap[new Vector2(x, y)] = new MapNode(x, y);
        return node;
    }

    public void RecalculateBounds()
    {
        bounds = new Rect();
        foreach (Vector2 point in map.Keys)
        {
            if (point.x < bounds.xMin)
                bounds.xMin = point.x - 1;
            else if (point.x > bounds.xMax)
                bounds.xMax = point.x + 1;
            if (point.y < bounds.yMin)
                bounds.yMin = point.y - 1;
            else if (point.y > bounds.yMax)
                bounds.yMax = point.y + 1;
        }
    }

    public bool Blocking(MapNode node)
    {
        if (!map.ContainsKey(node.ToVector2()))
            return false;
        List<GameObject> goList = map[node.ToVector2()];
        foreach (GameObject g in goList) 
            if (g != null && g.GetComponent<ObjectData>().type == ObjectType.Wall)
                return true;
        return false;
    }

    public float DistanceBetween(MapNode one, MapNode two)
    {
        return Vector2.Distance(new Vector2(one.x, one.y), new Vector2(two.x, two.y));
    }

    public List<MapNode> GetPotentialNeighbors(MapNode current)
    {
        List<MapNode> ret = new List<MapNode>();
        for (int x = current.x - 1; x <= current.x + 1; x++)
            for (int y = current.y - 1; y <= current.y + 1; y++)
            {
                if (!bounds.Contains(new Vector2(x, y)) || x == current.x && y == current.y)
                    continue;
                ret.Add(GetNode(x, y));
            }
        return ret;
    }

    public List<MapNode> GetNeighbors(MapNode current)
    {
        return GetPotentialNeighbors(current).FindAll((n) => {
            return !Blocking(n);
        });
    }
}
