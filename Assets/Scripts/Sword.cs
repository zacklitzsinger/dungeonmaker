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
    public float knockback;
    public bool friendly;
    public GameObject owner;
    public AudioClip swordSwingSound;
    public AudioClip swordHitSound;

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
        Health health = other.GetComponentInParent<Health>();
        Shield shield = other.GetComponent<Shield>();
        if (!health || !shield && other.isTrigger)
            return;
        hits.Add(other.gameObject);
        Vector2 direction = (other.transform.position - transform.position).normalized;
        if (shield)
        {
            shield.Block();
            other.GetComponentInParent<Rigidbody2D>().AddForce(direction * knockback);
        }
        else
            health.Damage(damage, owner, direction * knockback);
        if (ownerRb2d)
            ownerRb2d.AddForce(-direction * knockback);
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordHitSound);
        if (shield)
            Destroy(gameObject.transform.parent);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (remainingFrames > 8 || friendly == other.CompareTag("Player") || hits.Contains(other.gameObject))
            return;
        Vector2 direction = (other.transform.position - transform.position).normalized;
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        if (otherData.type != ObjectType.Wall)
            return;
        //TODO Animation or particle effect when sword bonks against a wall
        Destroy(gameObject);
        if (ownerRb2d)
            ownerRb2d.AddForce(-direction * knockback);
        return;
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
