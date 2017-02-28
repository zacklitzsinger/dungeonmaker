using UnityEngine;

public class Sword : MonoBehaviour
{
    public int remainingFrames;
    public int damage;
    public float knockback;
    public bool friendly;
    public AudioClip swordSwingSound;
    public AudioClip swordHitSound;

    void Start()
    {
        Camera.main.GetComponent<AudioSource>().PlayOneShot(swordSwingSound);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (friendly == other.CompareTag("Player"))
            return;
        Health health = other.GetComponentInParent<Health>();
        if (!health)
            return;
        Vector2 direction = (other.transform.position - transform.position).normalized;
        health.Damage(damage, direction * knockback);
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
