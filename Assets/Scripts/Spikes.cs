using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

    public bool active = false;
    public int framesDown;
    public int framesUp;
    [ReadOnly] public int remFrames;

    public int damage;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();
        if (health == null)
            return;
        health.Damage(damage);
    }

    void FixedUpdate()
    {
        remFrames--;
        if (remFrames <= 0)
        {
            if (active)
                remFrames = framesDown;
            else
                remFrames = framesUp;
            active = !active;
            GetComponent<Collider2D>().enabled = active;
        }
        animator.SetBool("active", active);
        
    }
}
