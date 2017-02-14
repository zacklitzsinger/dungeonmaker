using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class does modify the given NavNodes and does direct comparisons between them.
 * Do not construct more than one NavNode per actual node on the map.
 */
public class NavigationCalculator<T> where T : NavNode<T> {

    private INavMap<T> map_;

    public NavigationCalculator(INavMap<T> map)
    {
        map_ = map;
    }

    /**
     * Returns an ordered list of INavNodes from start to end.
     */
    public List<T> CalculatePath(T start, T end)
    {
        List<T> neighbors;
        List<T> closedNodes = new List<T>();
        // openNodes will always be sorted. Inserts should only be done via InsertIntoOpen.
        List<T> openNodes = new List<T>();
        T current = null;
        start.parent = null;
        start.distanceFromStart = 0;
        start.distanceToEnd = map_.DistanceBetween(start, end);
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
            neighbors = map_.GetNeighbors(current);
            foreach (T n in neighbors) {
                // Already been here
                if (closedNodes.Contains(n))
                    continue;
                distance = current.distanceFromStart + map_.DistanceBetween(current, n);
                // Already have as good / better path to node
                if (openNodes.Contains(n) && distance >= n.distanceFromStart)
                {
                    continue;
                }
                n.distanceFromStart = distance;
                n.distanceToEnd = map_.DistanceBetween(n, end);
                n.parent = current;
                // This will reinsert into the correct location if openNodes already contains n.
                InsertIntoOpen(openNodes, n);
            }
        }
        return null;
    }

    /**
     * Inserts into a sorted list. Removes first if the element is already there.
     */
    private void InsertIntoOpen(List<T> openNodes, T node)
    {
        if (openNodes.Contains(node))
        {
            openNodes.Remove(node);
        }
        float fscore = node.distanceFromStart + node.distanceToEnd;
        for (int i = 0; i < openNodes.Count; i++)
        {
            if (fscore < openNodes[i].distanceFromStart + openNodes[i].distanceToEnd)
            {
                openNodes.Insert(i, node);
                return;
            }
        }
        openNodes.Insert(openNodes.Count, node);
    }

    /**
     * Takes the final node and builds the path backwards from that node to the start.
     * Then, it reverses the resulting list to give a forward path.
     */
    private List<T> GetPath(T end)
    {
        T current = end;
        List<T> path = new List<T>();
        path.Add(current);
        while (current.parent != null)
        {
            current = current.parent;
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    /**
 * Returns all nodes connected to the given node.
 */
    public List<T> GetConnectedNodes(T start)
    {
        List<T> neighbors;
        // Our eventual list of connected nodes.
        List<T> closedNodes = new List<T>();
        List<T> openNodes = new List<T>();
        T current = null;
        openNodes.Add(start);
        while (openNodes.Count > 0)
        {
            current = openNodes[0];
            openNodes.Remove(current);
            closedNodes.Add(current);
            neighbors = map_.GetNeighbors(current);
            foreach (T n in neighbors)
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
