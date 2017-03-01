using UnityEngine;

public class Fire : MonoBehaviour
{

    [ReadOnly]
    public bool active = true;
    public float size = 5;

    void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health == null)
            return;
        health.Damage(1, gameObject, Vector2.zero);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health == null)
            return;
        health.Damage(1, gameObject, Vector2.zero);
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
        main.startSpeedMultiplier = (size + 1) * 1.25f;
        if (active && size > 0)
            ps.Play();
        else
            ps.Stop();
    }

}
