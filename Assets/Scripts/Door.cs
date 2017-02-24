using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Door : MonoBehaviour, ICustomSerializable {

    public bool open = false;

    [PlayerEditable("Invert")]
    public bool invert = false;

    Animator animator;
    public GameObject child;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && GetComponent<Circuit>() == null && !other.isTrigger)
            open = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player" && GetComponent<Circuit>() == null && !other.isTrigger)
            open = false;
    }

    void Update()
    {
        animator.SetBool("open", open);
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            open = circuit.Powered ^ invert;
        child.layer = open ? LayerMask.NameToLayer("IgnorePlayer") : LayerMask.NameToLayer("Default");
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
