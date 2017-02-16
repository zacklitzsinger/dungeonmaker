using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.collider.tag != "Player")
            return;
        int dmg = collision.collider.GetComponent<Health>().Damage(1);
        if (dmg > 0)
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            collision.rigidbody.AddForce(dir * 1200f);
        }
    }

}
