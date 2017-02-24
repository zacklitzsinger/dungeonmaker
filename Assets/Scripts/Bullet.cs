using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float velocity;
    public int lifetime;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(velocity * transform.up, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger)
            return;
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        if (!other.CompareTag("Player") && otherData != null && (otherData.type == ObjectType.Wall || otherData.type == ObjectType.Enemy))
        {
            Destroy(gameObject);
            Health otherHealth = other.GetComponent<Health>();
            if (otherHealth)
                otherHealth.Damage(1);
        }
    }

    void FixedUpdate()
    {
        if (lifetime-- <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

}
