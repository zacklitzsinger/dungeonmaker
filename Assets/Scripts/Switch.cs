using System;
using System.IO;
using UnityEngine;

public class Switch : MonoBehaviour, ICustomSerializable
{

    [PlayerEditable("Permanent")]
    public bool permanent = false;
    [PlayerEditable("Invert")]
    public bool invert = false;
    public bool active = false;
    [ReadOnly]
    public int count;

    Animator animator;
    Circuit circuit;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<ObjectData>() == null)
            return;
        active = true;
        count++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<ObjectData>() == null)
            return;
        if (permanent)
            return;
        count--;
        if (count <= 0)
            active = false;
    }

    void Update()
    {
        animator.SetBool("active", active);
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
            if (circuit != null)
                SetupCircuit();
        }
    }

    void SetupCircuit()
    {
        circuit.gateConditions.Add(() => { return active ^ invert; });
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
