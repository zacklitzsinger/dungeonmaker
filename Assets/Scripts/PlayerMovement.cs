using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    Rigidbody2D rb2d;
    public LevelEditor editor;
    public float acceleration;
    public float rollFrames; // Number of frames it takes to roll
    public float rollForce; // Force with which to roll

    public float remRollFrames; // Remaining frames to continue rolling; 0 unless rolling

    public int keys = 0;

    void Awake ()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate ()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetMotion = Vector2.right * xMotion + Vector2.up * yMotion;

        if (remRollFrames > 0)
        {
            rb2d.AddForce(targetMotion.normalized * rollForce / (rollFrames - remRollFrames + 1));
            remRollFrames--;
            return;
        }
        else
        {
            remRollFrames = 0;
        }

        if (Input.GetButtonDown("Roll"))
        {
            remRollFrames = rollFrames;
        }
        else
        {
            rb2d.AddForce(targetMotion * acceleration);
        }
	}
}
