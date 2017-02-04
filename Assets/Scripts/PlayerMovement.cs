using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    Rigidbody2D rb2d;
    public float acceleration;

    public int keys = 0;

    void Awake ()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate ()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetForce = Vector2.right * xMotion * acceleration + Vector2.up * yMotion * acceleration;
        rb2d.AddForce(targetForce);
	}
}
