using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour {

    public bool active = false;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        active = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        active = false;
    }

    void FixedUpdate()
    {
        animator.SetBool("active", active);

    }
}
