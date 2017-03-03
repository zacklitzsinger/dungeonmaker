using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollAttack : MonoBehaviour {

    public enum State
    {
        Delay,
        Attack
    }

    [ReadOnly]
    public State currentState = State.Delay;

    public Vector2 direction;
    public float force;
    public float breakSpeed;

    public int frameDelay = 30;
    int remFrames = 0;

    Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        remFrames = frameDelay;
        currentState = State.Delay;
    }

    void OnEnable()
    {
        direction = transform.up;
    }

    void FixedUpdate()
    {
        if (currentState == State.Delay)
        {
            if (remFrames-- < 0)
            {
                currentState = State.Attack;
                rb2d.AddForce(direction * force);
            }
        }
        else if (rb2d.velocity.magnitude < breakSpeed)
        {
            enabled = false;
            remFrames = frameDelay;
            currentState = State.Delay;
        }
    }

}
