using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Bridge : MonoBehaviour {

    public bool active = false;

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
        GetComponent<Collider2D>().enabled = active;
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(invert);
    }

    public void Deserialize(BinaryReader br)
    {
        invert = br.ReadBoolean();
    }
}
