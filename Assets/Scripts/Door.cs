using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool open = false;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            open = circuit.Powered;
        GetComponent<Collider2D>().enabled = !open;
        animator.SetBool("open", open);
    }
}
