using System;
using System.IO;
using UnityEngine;

public class StickyGate : MonoBehaviour, ICustomSerializable
{

    [PlayerEditableRange("Stickiness", 1, 30)]
    public int stickiness;
    [ReadOnly]
    public int remFrames;

    Circuit circuit;
    bool on = false;

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
        if (circuit != null)
            if (circuit.IncomingPower > 0)
            {
                remFrames = stickiness * 60;
                on = true;
            }
            else if (--remFrames <= 0)
            {
                remFrames = 0;
                on = false;
            }
    }

    void SetupCircuit()
    {
        circuit.powerConditions.Add(() => { return on; });
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
