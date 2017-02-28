using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    public bool active = false;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.isTrigger)
            return;
        Health health = other.GetComponentInParent<Health>();
        if (health == null)
            return;
        health.SetDeathRespawnPoint(this);
        active = true;
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

}
