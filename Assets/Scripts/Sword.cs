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

    Rigidbody2D rb2d;

    void Start()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordSwingSound);
        rb2d = owner.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (friendly == other.CompareTag("Player"))
            return;
        Health health = other.GetComponentInParent<Health>();
        // TODO Perhaps trigger knockback when the sword hits a wall?
        if (!health)
            return;
        Vector2 direction = (other.transform.position - transform.position).normalized;
        health.Damage(damage, owner, direction * knockback);
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
