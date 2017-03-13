using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Delay : MonoBehaviour, ICustomSerializable
{

    /// <summary>
    /// Number of seconds to delay inputs.
    /// </summary>
    [PlayerEditableRange("Delay", 0, 30)]
    public int delaySeconds = 1;
    /// <summary>
    /// Number of frames to delay inputs.
    /// </summary>
    public int DelayFrames { get { return delaySeconds * 60; } }
    Circuit circuit;
    Queue<bool> history = new Queue<bool>();

    void Start()
    {
        SetupCircuit();
    }

    void Update()
    {
        SetupCircuit();
    }

    void FixedUpdate()
    {
        if (circuit)
        {
            history.Enqueue(circuit.powerAmount > 1);
            if (history.Count > DelayFrames)
                history.Dequeue();
        }
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
        if (circuit == null)
            return;
        circuit.gateConditions.Add(() => { return DelayFrames == 0 || history.Count >= DelayFrames && history.Peek(); });
        circuit.powerConditions.Add(() => { return true; });
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
