using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    Rigidbody2D rb2d;
    public float acc;

    public int keys = 0;

    void Awake ()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate ()
    {
        float xMotion = Input.GetAxis("Horizontal");
        float yMotion = Input.GetAxis("Vertical");
        Vector2 targetForce = Vector2.right * xMotion * acc + Vector2.up * yMotion * acc;
        rb2d.AddForce(targetForce);
	}
}
