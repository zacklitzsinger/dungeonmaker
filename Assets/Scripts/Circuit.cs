using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Circuit : MonoBehaviour, ICustomSerializable
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
    [ReadOnly]
    public bool lastGateTest = false;
    [ReadOnly]
    public bool lastPowerTest = false;


    [ReadOnly]
    public List<Circuit> inputs = new List<Circuit>();
    /// <summary>
    /// List of functions that any of which being true will cause this node to produce power.
    /// </summary>
    [ReadOnly]
    public List<Func<bool>> powerConditions = new List<Func<bool>>();
    /// <summary>
    /// List of functions that must all return true in order for power to pass through the gate.
    /// </summary>
    [ReadOnly]
    public List<Func<bool>> gateConditions = new List<Func<bool>>();
    [ReadOnly]
    public List<Circuit> outputs = new List<Circuit>();

    public bool SelfPowered { get { return lastPowerTest; } }
    public int IncomingPower { get { return powerAmount - (lastPowerTest ? 1 : 0); } }

    void Awake()
    {
        powerConditions.Add(() =>
        {
            // A circuit with no inputs powers itself.
            return (inputs.Count == 0);
        });
    }

    /// <summary>
    /// Connects a circuit to another circuit one way.
    /// </summary>
    /// <param name="other"></param>
    public void Connect(Circuit other)
    {
        if (other == this)
            return;
        if (!outputs.Contains(other))
            outputs.Add(other);
        if (!other.inputs.Contains(this))
            other.inputs.Add(this);
    }

    void OnDestroy()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        foreach (Circuit input in inputs)
            input.outputs.Remove(this);
        inputs.Clear();
        foreach (Circuit output in outputs)
            output.inputs.Remove(this);
        outputs.Clear();
    }

    bool TestGateConditions()
    {
        foreach (Func<bool> condition in gateConditions)
            if (!condition())
                return false;
        return true;
    }

    bool TestPowerConditions()
    {
        foreach (Func<bool> condition in powerConditions)
            if (condition())
                return true;
        return false;
    }

    void FixedUpdate()
    {
        bool powerTest = TestPowerConditions();
        int powerDelta = 0;
        // A circuit with no inputs powers itself.
        if (powerTest && !lastPowerTest)
            powerDelta++;
        else if (!powerTest && lastPowerTest)
            powerDelta--;
        lastPowerTest = powerTest;

        DeterminePower(powerDelta);
    }

    /// <summary>
    /// Determines how much power needs to be transmitted. Can be checked in FixedUpdate() or when power is received from an input.
    /// </summary>
    /// <param name="power"></param>
    void DeterminePower(int power = 0, List<Circuit> visitedNodes = null)
    {
        int powerDelta = 0;
        // Keep track of the amount of power before we changed it in case we need to transmit that.
        int prevPowerAmount = powerAmount;
        powerAmount += power;
        bool test = TestGateConditions();
        if (outputs.Count > 0)
        {
            // If the gate is true, new incoming power should always go to output
            if (test)
                powerDelta += power;
            // In the case of a change of the gate, we should send power based on how much power we had 
            // BEFORE we modified it (since that was the part that has already been sent or needs to be sent).
            if (!test && lastGateTest)
                powerDelta -= prevPowerAmount;
            else if (test && !lastGateTest)
                powerDelta += prevPowerAmount;
        }
        lastGateTest = test;
        TransmitPower(powerDelta, visitedNodes);
    }

    void TransmitPower(int power, List<Circuit> visitedNodes = null)
    {
        if (power == 0)
            return;
        if (visitedNodes == null)
            visitedNodes = new List<Circuit>();
        visitedNodes.Add(this);
        foreach (Circuit output in outputs)
        {
            if (visitedNodes.Contains(output))
                continue;
            output.DeterminePower(power, visitedNodes);
        }
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(inputs.Count);
        foreach (Circuit input in inputs)
            bw.Write(input.GetComponent<ObjectData>().guid);
        bw.Write(outputs.Count);
        foreach (Circuit output in outputs)
            bw.Write(output.GetComponent<ObjectData>().guid);
    }

    public void Deserialize(BinaryReader br)
    {
        int inputsCount = br.ReadInt32();
        for (int i = 0; i < inputsCount; i++)
        {
            Guid id = br.ReadGuid();
            GameObject go = LevelEditor.main.guidmap[id];
            inputs.Add(go.GetComponent<Circuit>() ?? go.AddComponent<Circuit>());
        }
        int outputsCount = br.ReadInt32();
        for (int i = 0; i < outputsCount; i++)
        {
            Guid id = br.ReadGuid();
            GameObject go = LevelEditor.main.guidmap[id];
            outputs.Add(go.GetComponent<Circuit>() ?? go.AddComponent<Circuit>());
        }
    }
}
