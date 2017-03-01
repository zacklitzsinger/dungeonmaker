using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public int remainingFrames;
    public int damage;
    public float knockback;
    public bool friendly;
    public GameObject owner;
    public AudioClip swordSwingSound;
    public AudioClip swordHitSound;

    HashSet<GameObject> hits = new HashSet<GameObject>();

    Rigidbody2D rb2d;

    void Start()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordSwingSound);
        rb2d = owner.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (friendly == other.CompareTag("Player") || hits.Contains(other.gameObject))
            return;
        Vector2 direction = (other.transform.position - transform.position).normalized;
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        if (otherData.type == ObjectType.Wall)
        {
            //TODO Animation or particle effect when sword bonks against a wall
            Destroy(gameObject);
            if (rb2d)
                rb2d.AddForce(-direction * knockback);
            return;
        }
        Health health = other.GetComponentInParent<Health>();
        if (!health)
            return;
        health.Damage(damage, owner, direction * knockback);
        hits.Add(other.gameObject);
        if (rb2d)
            rb2d.AddForce(-direction * knockback);
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordHitSound);
    }

    void Update()
    {
        if (remainingFrames > 0)
            remainingFrames--;
        else
            Destroy(transform.parent.gameObject);
    }
}
