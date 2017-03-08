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
        if (other.isTrigger)
            return;
        ObjectData otherData = other.GetComponentInParent<ObjectData>();
        Health otherHealth = other.GetComponent<Health>();
        if (otherData == null)
            return;
        if (otherData.CompareTag("Player") != friendly || otherData.type == ObjectType.Wall)
        {
            Destroy(gameObject);
            if (otherHealth)
                otherHealth.Damage(1, gameObject, transform.up * velocity);
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
