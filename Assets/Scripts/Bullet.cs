using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float velocity;
    public int lifetime;
    public float knockbackModifier;
    public bool friendly;

    ParticleSystem ps;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(velocity * transform.up, ForceMode2D.Impulse);
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        Health otherHealth = other.GetComponentInParent<Health>();
        Shield shield = other.GetComponent<Shield>();
        if (other.isTrigger && !shield || otherData == null)
            return;
        if (otherData.CompareTag("Player") != friendly || otherData.type == ObjectType.Wall)
        {
            Stop();
            if (otherHealth)
            {
                if (shield)
                    shield.Block(null);
                else
                    otherHealth.Damage(1, gameObject, transform.up * velocity * knockbackModifier);
            }
        }
    }

    void FixedUpdate()
    {
        if (lifetime-- <= 0)
            Stop();
    }

    void Stop()
    {
        enabled = false;
        if (ps)
            ps.Stop();
    }

}
