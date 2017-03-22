using UnityEngine;

public class Explosion : MonoBehaviour {

    [HideInInspector]
    public float radius;
    [HideInInspector]
    public int damage;
    [HideInInspector]
    public float knockback;
    public AudioClip sound;

    void Explode()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.isTrigger)
                continue;
            IDamageable hitHealth = hit.transform.GetComponentInParent<IDamageable>();
            if (hitHealth == null)
                continue;
            Vector2 dir = ((hitHealth as MonoBehaviour).transform.position - transform.position).normalized;
            hitHealth.Damage(damage, gameObject, dir * knockback, DamageType.Explosive);
        }
    }

    void FixedUpdate()
    {
        Explode();
        Camera.main.GetComponent<AudioSource>().PlayOneShot(sound);
        enabled = false;
    }
}
