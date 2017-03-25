using UnityEngine;

public class DamageOnTouch : MonoBehaviour {

    public int damage = 1;
    public float knockback = 1200f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;
        Vector2 dir = (collision.transform.position - transform.position).normalized;
        collision.collider.GetComponentInParent<IDamageable>().Damage(damage, gameObject, dir * knockback);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        Shield shield = other.GetComponent<Shield>();
        IDamageable health = other.GetComponentInParent<IDamageable>();
        if (!shield || health == null)
            return;
        Vector2 dir = (other.transform.position - transform.position).normalized;
        other.GetComponentInParent<Rigidbody2D>().AddForce(dir * knockback);
        GetComponentInParent<Rigidbody2D>().AddForce(-dir * knockback);
        shield.Block(GetComponentInParent<IActionQueue>());
    }
}
