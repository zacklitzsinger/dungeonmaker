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
    private bool active;
    public bool Active
    {
        get { return active; }
        set
        {
            if (active == value)
                return;
            active = value;
            AudioSource.PlayClipAtPoint(active ? soundPressed : soundUnpressed, transform.position);
        }
    }
    [ReadOnly]
    public HashSet<ObjectData> touching = new HashSet<ObjectData>();

    public AudioClip soundPressed;
    public AudioClip soundUnpressed;

    Animator animator;
    Circuit circuit;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        SetupCircuit();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData data = other.GetComponentInParent<ObjectData>();
        if (data == null || other.isTrigger)
            return;
        touching.Add(data);
        Active = true;
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
            Active = false;
    }

    void Update()
    {
        animator.SetBool("active", Active);

        SetupCircuit();
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
        if (circuit == null)
            return;
        circuit.gateConditions.Add(() => { return Active ^ invert; });
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
