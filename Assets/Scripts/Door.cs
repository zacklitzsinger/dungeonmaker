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

    void Awake()
    {
        animator = GetComponent<Animator>();
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
        GetComponent<Collider2D>().enabled = !open;
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
