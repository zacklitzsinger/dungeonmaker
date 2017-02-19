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
        health.Knockback((other.transform.position - transform.position).normalized * 300f);
    }

    void Update()
    {
        if (remainingFrames > 0)
            remainingFrames--;
        else
            Destroy(transform.parent.gameObject);
    }
}
