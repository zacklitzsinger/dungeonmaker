using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour {

    public float force;
    [ReadOnly]
    public bool active = true;
    public float size = 5;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger)
            return;
        Push(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.isTrigger)
            return;
        Push(other);
    }

    void Update()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, size);
        collider.offset = new Vector2(0, (size + 1) / 2);
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps == null)
            return;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTimeMultiplier = (size + 1) * 40;
        ParticleSystem.MainModule main = ps.main;
        main.startSpeedMultiplier = (size + 1) * 1.25f;
        if (active && size > 0)
            ps.Play();
        else
            ps.Stop();
    }

    void Push(Collider2D other)
    {
        Rigidbody2D rb2d = other.GetComponentInParent<Rigidbody2D>();
        if (!rb2d || !active)
            return;
        rb2d.AddForce(transform.up * force);
    }

}
