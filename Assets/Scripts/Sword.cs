using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public enum Style
    {
        Swing,
        Thrust
    }

    public Style style;
    public int remainingFrames;
    public int damage;
    public int stagger;
    public float knockback;
    public bool friendly;
    public GameObject owner;
    public AudioClip swordSwingSound;
    public AudioClip swordHitSound;
    public ParticleSystem blockParticles;

    HashSet<GameObject> hits = new HashSet<GameObject>();

    Rigidbody2D ownerRb2d;
    Animator animator;

    void Start()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordSwingSound);
        ownerRb2d = owner.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (friendly == other.CompareTag("Player") || hits.Contains(other.gameObject))
            return;
        IDamageable health = other.GetComponentInParent<IDamageable>();
        Shield shield = other.GetComponent<Shield>();
        if (health == null || !shield && other.isTrigger)
            return;
        hits.Add(other.gameObject);
        Vector2 direction = (other.transform.position - transform.position).normalized;
        if (shield)
        {
            shield.Block(GetComponentInParent<IActionQueue>());
            other.GetComponentInParent<Rigidbody2D>().AddForce(direction * knockback);
        }
        else
            health.Damage(damage, owner, direction * knockback, DamageType.Slash);
        IActionQueue otherAQ = other.GetComponentInParent<IActionQueue>();
        if (otherAQ != null)
            otherAQ.Interrupt(stagger);
        if (ownerRb2d)
        {
            ownerRb2d.AddForce(-direction * knockback);
            Instantiate(blockParticles, transform.position, Quaternion.LookRotation(direction, Vector3.forward));
        }
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordHitSound);
        if (shield)
            Destroy(transform.parent.gameObject);
    }

    // For hitting walls
    // TODO: Handle sword hitting multiple walls in a single frame.
    void OnTriggerStay2D(Collider2D other)
    {
        if (remainingFrames > 8 || friendly == other.CompareTag("Player") || other.isTrigger || hits.Contains(other.gameObject))
            return;
        Vector2 direction = (other.transform.position - transform.position).normalized;
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        if (!otherData || otherData.type != ObjectType.Wall)
            return;
        Destroy(transform.parent.gameObject);
        if (ownerRb2d)
        {
            Instantiate(blockParticles, transform.position, Quaternion.LookRotation(direction, Vector3.forward));
            ownerRb2d.AddForce(-direction * knockback);
        }
    }

    void Update()
    {
        animator.SetInteger("style", (int)style);
        if (remainingFrames > 0)
            remainingFrames--;
        else
            Destroy(transform.parent.gameObject);
    }
}
