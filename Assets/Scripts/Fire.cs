using UnityEngine;

public class Fire : MonoBehaviour
{

    [ReadOnly]
    public bool active = true;
    public float size = 5;
    public float knockback;

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable health = other.GetComponentInParent<IDamageable>();
        if (health == null || other.isTrigger)
            return;
        health.Damage(1, gameObject, -knockback * other.GetComponentInParent<Rigidbody2D>().velocity.normalized);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        IDamageable health = other.GetComponentInParent<IDamageable>();
        if (health == null || other.isTrigger)
            return;
        health.Damage(1, gameObject, -knockback * other.GetComponentInParent<Rigidbody2D>().velocity.normalized);
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
        main.startSpeedMultiplier = (size + 1) * .8f;
        if (active && size > 0)
            ps.Play();
        else
            ps.Stop();
    }

}
