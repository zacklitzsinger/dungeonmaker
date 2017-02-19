using System;
using System.Collections;
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

    bool TestConditions()
    {
        foreach(Func<bool> condition in conditions)
            if (!condition())
                return false;
        return true;
    }

    void FixedUpdate()
    {
        // A circuit with no inputs powers itself.
        if (inputs.Count == 0)
            powerAmount = 1;
        //else if (powerAmount > inputs.Count)
        //{
        //    Debug.LogWarning("Too much power! " + gameObject.name);
        //} else if (powerAmount < 0)
        //{
        //    Debug.LogWarning("Negative power! " + gameObject.name);
        //}

        DeterminePower();
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
        bool test = TestConditions();
        if (outputs.Count > 0)
        {
            // If the gate is true, new incoming power should always go to output
            if (test)
                powerDelta += power;
            // In the case of a change of the gate, we should send power based on how much power we had 
            // BEFORE we modified it (since that was the part that has already been sent or needs to be sent).
            if (!test && lastTest)
                powerDelta -= prevPowerAmount;
            else if (test && !lastTest)
                powerDelta += prevPowerAmount;
        }
        lastTest = test;
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
        foreach(Circuit input in inputs)
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
