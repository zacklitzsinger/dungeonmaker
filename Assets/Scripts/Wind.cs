﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour {

    public float force;
    public bool active = true;
    public float size = 5;

    void OnTriggerEnter2D(Collider2D other)
    {
        Push(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Push(other);
    }

    void Update()
    {
        GetComponent<BoxCollider2D>().size = new Vector2(1, size);
        transform.localPosition = new Vector2(0, (size + 1) / 2);
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (active)
            ps.Play();
        else
            ps.Stop();
    }

    void Push(Collider2D other)
    {
        Rigidbody2D rb2d = other.GetComponent<Rigidbody2D>();
        if (!rb2d || !active)
            return;
        rb2d.AddForce(transform.up * force);
    }

}