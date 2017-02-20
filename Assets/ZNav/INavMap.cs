using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Your map should implement this interface. It will provide all
 * of the data needed for the A* algorithm to work.
 */
public interface INavMap<T> where T : NavNode<T> {

    /**
     * Given the origin node, return a list of neighbors
     */
    List<T> GetNeighbors(T origin, bool includeEmpty, bool ignoreSeeThrough);

    /**
     * Returns the distance between two NavNodes
     */
    float DistanceBetween(T one, T two);

    void RecalculateBounds();
}
