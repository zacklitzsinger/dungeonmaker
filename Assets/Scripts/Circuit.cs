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
    public List<Circuit> inputs = new List<Circuit>();
    [ReadOnly]
    public List<Circuit> outputs = new List<Circuit>();

    public void Connect(Circuit other)
    {
        if (!outputs.Contains(other))
            outputs.Add(other);
        if (!other.inputs.Contains(this))
            other.inputs.Add(this);
    }

    public void AdjustPower(int power, List<Circuit> visitedNodes = null)
    {
        if (visitedNodes == null)
        {
            visitedNodes = new List<Circuit>();
        }
        visitedNodes.Add(this);
        powerAmount += power;
        foreach (Circuit output in outputs)
        {
            if (visitedNodes.Contains(output))
                continue;
            output.AdjustPower(power, visitedNodes);
        }
    }
}
