using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float velocity;
    public int lifetime;
    public bool friendly;

    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(velocity * transform.up, ForceMode2D.Impulse);
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
            Destroy(gameObject);
            if (otherHealth)
            {
                if (shield)
                    shield.Block(null);
                else
                    otherHealth.Damage(1, gameObject, transform.up * velocity * 50);
            }
        }
    }

    void FixedUpdate()
    {
        if (lifetime-- <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

}
