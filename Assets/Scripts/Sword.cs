using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{

    public int remainingFrames;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            return;
        Health health = other.GetComponent<Health>();
        if (!health)
            return;
        health.Damage(1);
        other.GetComponent<Rigidbody2D>().AddForce((other.transform.position - transform.position).normalized * 300f);
    }

    void Update()
    {
        if (remainingFrames > 0)
            remainingFrames--;
        else
            Destroy(gameObject);
    }
}
