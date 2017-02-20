using System.Collections.Generic;

/// <summary>
/// This class does modify the given NavNodes and does direct comparisons between them.
/// Do not construct more than one NavNode per actual node on the map.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NavigationCalculator<T> where T : NavNode<T> {

    private INavMap<T> map_;

    public NavigationCalculator(INavMap<T> map)
    {
        map_ = map;
    }

    /// <summary>
    /// Returns an ordered list of INavNodes from start to end.
    /// </summary>
    public List<T> CalculatePath(T start, T end, bool includeEmpty = false, bool ignoreSeeThrough = false)
    {
        map_.RecalculateBounds();
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
            neighbors = map_.GetNeighbors(current, includeEmpty, ignoreSeeThrough);
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

    /// <summary>
    /// Inserts into a sorted list. Removes first if the element is already there.
    /// </summary>
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

    /// <summary>
    /// Takes the final node and builds the path backwards from that node to the start. Then, it reverses the resulting list to give a forward path.
    /// </summary>
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

    /// <summary>
    /// Returns all nodes connected to the given node.
    /// </summary>
    public List<T> GetConnectedNodes(T start, bool includeEmpty = false, bool ignoreSeeThrough = false)
    {
        map_.RecalculateBounds();
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
            neighbors = map_.GetNeighbors(current, includeEmpty, ignoreSeeThrough);
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
