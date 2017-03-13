using UnityEngine;

public class Explosive : MonoBehaviour
{

    public float radius;
    public float knockback;
    public int damage;

    Health health;
    Circuit circuit;

    // Use this for initialization
    void Start()
    {
        health = GetComponent<Health>();
        health.onDamaged += (go) =>
        {
            Explode();
        };
    }

    void Explode()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            Health hitHealth = hit.transform.GetComponentInParent<Health>();
            if (!hitHealth || hitHealth.gameObject == gameObject)
                continue;
            Vector2 dir = (hitHealth.transform.position - transform.position).normalized;
            hitHealth.Damage(damage, gameObject, dir * knockback, DamageType.Explosive);

        }
    }

    void FixedUpdate()
    {
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
        }
        if (circuit && circuit.Powered)
        {
            health.Damage(1, gameObject, Vector2.zero);
        }
    }
}
