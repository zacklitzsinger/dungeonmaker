using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    public bool active = false;

    Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.isTrigger)
            return;
        IRespawnable respawn = other.GetComponentInParent<IRespawnable>();
        if (respawn == null)
            return;
        respawn.SetDeathRespawnPoint(this);
        active = true;
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

}
