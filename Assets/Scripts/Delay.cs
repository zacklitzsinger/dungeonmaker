using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// TODO: GET WORKING
public class Delay : MonoBehaviour, ICustomSerializable
{

    /// <summary>
    /// Number of frames to delay inputs.
    /// </summary>
    [PlayerEditableRange("Delay", 0, 300)]
    public int delay;
    Circuit circuit;
    Queue<bool> history = new Queue<bool>();

    void Update()
    {
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
            if (circuit != null)
                SetupCircuit();
        }
    }

    void FixedUpdate()
    {
        if (circuit)
        {
            history.Enqueue(circuit.Powered);
            if (history.Count > delay)
                history.Dequeue();
        }
    }

    void SetupCircuit()
    {
        circuit.gateConditions.Add(() => { return delay == 0 || history.Count >= delay && history.Peek(); });
    }

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }
}
