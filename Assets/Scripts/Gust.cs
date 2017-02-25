using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gust : MonoBehaviour {

    public float force;
    public int aliveFrames;
    public bool active = true;

    ParticleSystem ps;

    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
    }

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
        if (active)
            ps.Play();
        else
            ps.Stop();

        gameObject.layer = active ? LayerMask.NameToLayer("Wind") : LayerMask.NameToLayer("CollisionDisabled");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (aliveFrames-- <= 0)
            Destroy(gameObject);
    }


    void Push(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;
        if (other.isTrigger || !active)
            return;
        if (other.GetComponentInParent<ObjectData>().type == ObjectType.Wall)
        {
            active = false;
            return;
        }
        Rigidbody2D rb2d = other.GetComponentInParent<Rigidbody2D>();
        if (!rb2d)
            return;
        rb2d.AddForce(transform.up * force);
        
    }
}
