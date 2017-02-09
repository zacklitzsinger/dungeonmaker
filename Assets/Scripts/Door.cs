using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public bool open = false;

    Circuit circuit;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        circuit = GetComponent<Circuit>();
    }


    void FixedUpdate()
    {
        open = circuit.Powered;
        GetComponent<Collider2D>().enabled = !open;
        animator.SetBool("open", open);
    }
}
