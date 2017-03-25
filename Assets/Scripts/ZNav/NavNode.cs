using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class should be extended to represent an actual node in an INavMap.
 * These internal variables should not be used (they're used by NavigationCalculator exclusively).
 */
public abstract class NavNode<T> where T : NavNode<T> {
    internal float distanceFromStart;
    internal float distanceToEnd;
    internal T parent;
}
