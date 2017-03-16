using UnityEngine;

public class HeartPickup : MonoBehaviour {

    public AudioClip sound;
    public int amount;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            IDamageable health = other.GetComponentInParent<IDamageable>();
            if (health.Heal(amount))
            {
                Camera.main.GetComponent<AudioSource>().PlayOneShot(sound);
                Destroy(gameObject);
            }
        }
    }

}
