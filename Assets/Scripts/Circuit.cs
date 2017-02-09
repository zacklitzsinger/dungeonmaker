using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
    [ReadOnly]
    public int powerAmount;
    public bool Powered
    {
        get
        {
            return powerAmount > 0;
        }
    }
    [ReadOnly]
    public List<Circuit> connections = new List<Circuit>();

    public bool Connect(Circuit other)
    {
        if (connections.Contains(other))
            return false;
        connections.Add(other);
        return true;
    }

    public void AdjustPower(int power, List<Circuit> visitedNodes = null)
    {
        if (visitedNodes == null)
        {
            visitedNodes = new List<Circuit>();
        }
        visitedNodes.Add(this);
        powerAmount += power;
        foreach (Circuit connection in connections)
        {
            if (visitedNodes.Contains(connection))
                continue;
            connection.AdjustPower(power, visitedNodes);
        }
    }
}
