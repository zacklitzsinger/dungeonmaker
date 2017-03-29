using UnityEngine;

public class Explosive : MonoBehaviour
{

    public float radius;
    public float knockback;
    public int damage;
    public Explosion explosionPrefab;

    Health health;
    Circuit circuit;

    // Use this for initialization
    void Start()
    {
        health = GetComponent<Health>();
        health.onDeath += (go) =>
        {
            Explosion explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.radius = radius;
            explosion.knockback = knockback;
            explosion.damage = damage;
        };
    }

    void FixedUpdate()
    {
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
        }
        if (circuit && circuit.Powered && circuit.IncomingPower > 0)
        {
            health.Die();
        }
    }
}
