using System;
using System.IO;
using UnityEngine;

public class Switch : MonoBehaviour, ICustomSerializable
{

    [PlayerEditable("Permanent")]
    public bool permanent = false;
    public bool active = false;
    [ReadOnly]
    public int count;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        active = true;
        count++;
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            circuit.AdjustPower(1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (permanent)
            return;
        count--;
        if (count <= 0)
        {
            active = false;
            Circuit circuit = GetComponent<Circuit>();
            if (circuit)
                circuit.AdjustPower(-1);
        }
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(permanent);
    }

    public void Deserialize(BinaryReader br)
    {
        permanent = br.ReadBoolean();
    }
}
