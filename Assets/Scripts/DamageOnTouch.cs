using UnityEngine;

public class DamageOnTouch : MonoBehaviour {

    public int damage = 1;
    public float knockback = 1200f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;
        Vector2 dir = (collision.transform.position - transform.position).normalized;
        collision.collider.GetComponentInParent<Health>().Damage(damage, gameObject, dir * knockback);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        Shield shield = other.GetComponent<Shield>();
        Health health = other.GetComponentInParent<Health>();
        if (!shield || !health)
            return;
        Vector2 dir = (other.transform.position - transform.position).normalized;
        other.GetComponentInParent<Rigidbody2D>().AddForce(dir * knockback / 5);
        GetComponentInParent<Rigidbody2D>().AddForce(-dir * knockback / 5);
        shield.Block();
    }
}
