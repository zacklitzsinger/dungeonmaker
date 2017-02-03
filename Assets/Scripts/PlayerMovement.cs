using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    Rigidbody2D rb2d;
    public float acc;
    public float maxSpeed;

    void Awake ()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
	
	void FixedUpdate ()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        rb2d.AddForce(Vector2.right * xMotion * acc);
        rb2d.AddForce(Vector2.up * yMotion * acc);
        Vector2 vel = rb2d.velocity;
        if (Mathf.Abs(vel.x) > maxSpeed)
            vel.x = Mathf.Sign(vel.x) * maxSpeed;
        if (Mathf.Abs(vel.y) > maxSpeed)
            vel.y = Mathf.Sign(vel.y) * maxSpeed;
        rb2d.velocity = vel;
	}
}
