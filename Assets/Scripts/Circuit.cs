using System;
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
    // Test conditions last frame
    public bool lastTest = false;


    [ReadOnly]
    public List<Circuit> inputs = new List<Circuit>();
    [ReadOnly]
    public List<Func<bool>> conditions = new List<Func<bool>>();
    [ReadOnly]
    public List<Circuit> outputs = new List<Circuit>();

    /// <summary>
    /// Connects a circuit to another circuit one way.
    /// </summary>
    /// <param name="other"></param>
    public void Connect(Circuit other)
    {
        if (!outputs.Contains(other))
            outputs.Add(other);
        if (!other.inputs.Contains(this))
            other.inputs.Add(this);
    }

    bool TestConditions()
    {
        foreach(Func<bool> condition in conditions)
            if (!condition())
                return false;
        return true;
    }

    void FixedUpdate()
    {
        if (inputs.Count == 0)
            powerAmount = 1;

        bool test = TestConditions();
        if (test && !lastTest)
            TransmitPower(powerAmount);
        if (!test && lastTest)
            TransmitPower(-powerAmount);
        lastTest = test;
    }

    public void AdjustPower(int power, List<Circuit> visitedNodes = null)
    {
        if (visitedNodes == null)
            visitedNodes = new List<Circuit>();
        visitedNodes.Add(this);
        powerAmount += power;
        if (!TestConditions())
            return;
        TransmitPower(power, visitedNodes);
    }

    void TransmitPower(int power, List<Circuit> visitedNodes = null)
    {
        if (visitedNodes == null)
            visitedNodes = new List<Circuit>();
        visitedNodes.Add(this);
        foreach (Circuit output in outputs)
        {
            if (visitedNodes.Contains(output))
                continue;
            output.AdjustPower(power, visitedNodes);
        }
    }
}
