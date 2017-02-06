using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Rolling,
    Attacking
}

public class Player : MonoBehaviour {

    Rigidbody2D rb2d;
    public LevelEditor editor;
    public Sword sword;

    public float acceleration;
    public int rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll

    public int remStateFrames; // Remaining frames to continue current state; 0 when in idle

    public int keys = 0;

    public PlayerState state = PlayerState.Idle;

    void Awake ()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate ()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;

        if (remStateFrames > 0)
        {
            remStateFrames--;
            if (state == PlayerState.Rolling)
                rb2d.AddForce(targetMotion.normalized * rollForce / (rollFrames - remStateFrames + 1));
            return;
        }
        else
        {
            remStateFrames = 0;
            state = PlayerState.Idle;
        }

        if (Input.GetButtonDown("Roll") && state == PlayerState.Idle)
        {
            remStateFrames = rollFrames;
            state = PlayerState.Rolling;
        }
        else
        {
            rb2d.AddForce(targetMotion * acceleration);
        }

        if (Input.GetButtonDown("Attack"))
        {
            Vector2 targetDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            targetDirection.Normalize();
            Sword s = Instantiate<Sword>(sword, (Vector2)transform.position + targetDirection, Quaternion.LookRotation(Vector3.forward, targetDirection), transform);
            remStateFrames = s.remainingFrames;
        }
    }
}
