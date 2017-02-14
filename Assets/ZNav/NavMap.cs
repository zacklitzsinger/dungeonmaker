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
}

public class NavMap : INavMap<MapNode>
{
    public Dictionary<MapNode, bool> map = new Dictionary<MapNode, bool>();
    int xMin, xMax, yMin, yMax;

    public void Add(Vector2 point)
    {
        MapNode node = new MapNode(point);
        if (map.ContainsKey(node))
            return;
        map[node] = true;
        ExtendBorders(node);
    }

    public void Remove(Vector2 point)
    {
        if (map.Remove(new MapNode(point)))
            RecalculateBorders();
    }

    void RecalculateBorders()
    {
        xMin = xMax = yMin = yMax = 0;
        foreach(MapNode node in map.Keys)
            ExtendBorders(node);
    }

    void ExtendBorders(MapNode node)
    {
        if (node.x < xMin)
            xMin = node.x;
        else if (node.x > xMax)
            xMax = node.x;
        if (node.y < yMin)
            yMin = node.y;
        else if (node.y > yMax)
            yMax = node.y;
    }

    public float DistanceBetween(MapNode one, MapNode two)
    {
        return Vector2.Distance(new Vector2(one.x, one.y), new Vector2(two.x, two.y));
    }

    public List<MapNode> GetPotentialNeighbors(MapNode current)
    {
        List<MapNode> ret = new List<MapNode>();
        for (int x = current.x - 1; x < current.x + 2; x++)
        {
            for (int y = current.y - 1; y < current.y + 2; y++)
            {
                if (x == current.x && y == current.y)
                    continue;
                MapNode node = new MapNode(x, y);
                ret.Add(node);
            }
        }
        return ret;
    }

    public List<MapNode> GetNeighbors(MapNode current)
    {
        return GetPotentialNeighbors(current).FindAll((n) => { return map.ContainsKey(n); });
    }
}
