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
        if (other.tag != "Player")
            return;
        other.GetComponent<Health>().SetRespawnPoint(this);
        active = true;
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

}
