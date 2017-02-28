using System.Collections.Generic;
using UnityEngine;


public class NavigationCalculator {

    private NavMap map;
    private Dictionary<Vector2, NodeMetadata> metadata = new Dictionary<Vector2, NodeMetadata>();

    struct NodeMetadata
    {
        public float distanceFromStart;
        public float distanceToEnd;
        public Vector2? parent;
    }

    public NavigationCalculator(NavMap map)
    {
        this.map = map;
    }

    /// <summary>
    /// Returns an ordered list of INavNodes from start to end.
    /// </summary>
    public List<Vector2> CalculatePath(Vector2 start, Vector2 end, bool includeEmpty = false, bool ignoreSeeThrough = false)
    {
        start = start.ToGrid();
        end = end.ToGrid();
        map.RecalculateBounds();
        metadata.Clear();
        List<Vector2> neighbors;
        List<Vector2> closedNodes = new List<Vector2>();
        // openNodes will always be sorted. Inserts should only be done via InsertIntoOpen.
        List<Vector2> openNodes = new List<Vector2>();
        Vector2 current;
        metadata[start] = new NodeMetadata() { parent = null, distanceFromStart = 0, distanceToEnd = map.DistanceBetween(start, end) };
        InsertIntoOpen(openNodes, start);
        float distance;
        while (openNodes.Count > 0)
        {
            // Guaranteed to be the lowest fscore.
            current = openNodes[0];
            if (current == end)
            {
                return GetPath(current);
            }
            openNodes.Remove(current);
            closedNodes.Add(current);
            neighbors = map.GetNeighbors(current, includeEmpty, ignoreSeeThrough);
            foreach (Vector2 n in neighbors) {
                // Already been here
                if (closedNodes.Contains(n))
                    continue;
                distance = metadata[current].distanceFromStart + map.DistanceBetween(current, n);
                // Already have as good / better path to node
                if (openNodes.Contains(n) && distance >= metadata[n].distanceFromStart)
                {
                    continue;
                }
                metadata[n] = new NodeMetadata() { parent = current, distanceFromStart = distance, distanceToEnd = map.DistanceBetween(n, end) };
                // This will reinsert into the correct location if openNodes already contains n.
                InsertIntoOpen(openNodes, n);
            }
        }
        return null;
    }

    /// <summary>
    /// Inserts into a sorted list. Removes first if the element is already there.
    /// </summary>
    private void InsertIntoOpen(List<Vector2> openNodes, Vector2 node)
    {
        if (openNodes.Contains(node))
        {
            openNodes.Remove(node);
        }
        float fscore = metadata[node].distanceFromStart + metadata[node].distanceToEnd;
        for (int i = 0; i < openNodes.Count; i++)
        {
            if (fscore < metadata[openNodes[i]].distanceFromStart + metadata[openNodes[i]].distanceToEnd)
            {
                openNodes.Insert(i, node);
                return;
            }
        }
        openNodes.Insert(openNodes.Count, node);
    }

    /// <summary>
    /// Takes the final node and builds the path backwards from that node to the start. Then, it reverses the resulting list to give a forward path.
    /// </summary>
    private List<Vector2> GetPath(Vector2 end)
    {
        Vector2 current = end;
        List<Vector2> path = new List<Vector2>();
        path.Add(current);
        while (metadata[current].parent != null)
        {
            current = (Vector2)metadata[current].parent;
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Returns all nodes connected to the given node.
    /// </summary>
    public List<Vector2> GetConnectedNodes(Vector2 start, bool includeEmpty = false, bool ignoreSeeThrough = false)
    {
        map.RecalculateBounds();
        List<Vector2> neighbors;
        // Our eventual list of connected nodes.
        List<Vector2> closedNodes = new List<Vector2>();
        List<Vector2> openNodes = new List<Vector2>();
        Vector2 current;
        openNodes.Add(start);
        while (openNodes.Count > 0)
        {
            current = openNodes[0];
            openNodes.Remove(current);
            closedNodes.Add(current);
            neighbors = map.GetNeighbors(current, includeEmpty, ignoreSeeThrough);
            foreach (Vector2 n in neighbors)
            {
                // Already been here
                if (openNodes.Contains(n) || closedNodes.Contains(n))
                    continue;
                openNodes.Add(n);
            }
        }
        return closedNodes;
    }
}
