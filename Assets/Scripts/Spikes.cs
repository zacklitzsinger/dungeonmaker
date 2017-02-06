using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour {

    public bool active = false;
    public int framesDown;
    public int framesUp;
    public int remFrames;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
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
        }
        animator.SetBool("active", active);
        
    }
}
