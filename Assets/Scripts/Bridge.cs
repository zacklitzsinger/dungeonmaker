using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Bridge : MonoBehaviour {

    public bool active = true;

    [PlayerEditable("Invert")]
    public bool invert = false;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            active = circuit.Powered ^ invert;
        gameObject.layer = active ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("CollisionDisabled");
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
