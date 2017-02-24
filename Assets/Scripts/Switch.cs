using System;
using System.Collections.Generic;
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
    public HashSet<ObjectData> touching = new HashSet<ObjectData>();

    Animator animator;
    Circuit circuit;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData data = other.GetComponentInParent<ObjectData>();
        if (data == null || other.isTrigger)
            return;
        active = true;
        touching.Add(data);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (permanent)
            return;
        ObjectData data = other.GetComponentInParent<ObjectData>();
        if (data == null || other.isTrigger)
            return;
        touching.Remove(data);
        if (touching.Count <= 0)
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
