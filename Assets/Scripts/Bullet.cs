using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float velocity;
    public int lifetime;
    public int damage;
    public float knockbackModifier;
    public bool friendly;
    public float charge;
    public float chargeVelocityModifier;
    public float chargeSizeModifier;
    public float chargeDamageModifier;

    public AudioClip sound;

    ParticleSystem ps;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(Mathf.Lerp(velocity, velocity * chargeVelocityModifier, charge) * transform.up, ForceMode2D.Impulse);
        ps = GetComponentInChildren<ParticleSystem>();
        if (sound)
            AudioSource.PlayClipAtPoint(sound, transform.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        IDamageable otherHealth = other.GetComponentInParent<IDamageable>();
        Shield shield = other.GetComponent<Shield>();
        if (other.isTrigger && !shield || otherData == null)
            return;
        if (otherData.CompareTag("Player") != friendly || otherData.type == ObjectType.Wall)
        {
            Stop();
            if (otherHealth != null)
            {
                if (shield)
                    shield.Block(null);
                else
                {
                    int dmg = Mathf.FloorToInt(Mathf.Lerp(damage, damage * chargeDamageModifier, charge));
                    otherHealth.Damage(dmg, gameObject, transform.up * velocity * knockbackModifier);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (ps)
            ps.gameObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * chargeSizeModifier, charge);
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
