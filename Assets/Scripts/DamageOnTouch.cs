using UnityEngine;

public class DamageOnTouch : MonoBehaviour {

    public int damage = 1;
    public float knockback = 1200f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;
        Vector2 dir = (collision.transform.position - transform.position).normalized;
        collision.collider.GetComponentInParent<Health>().Damage(damage, dir * knockback);
    }
}
