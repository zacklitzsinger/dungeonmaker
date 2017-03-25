using UnityEngine;

public class Spikes : MonoBehaviour {

    public bool active = false;
    public int framesDown;
    public int framesUp;
    [ReadOnly] public int remFrames;

    public int damage;
    public float knockback;

    Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable health = other.GetComponentInParent<IDamageable>();
        if (health == null)
            return;
        health.Damage(damage, gameObject, (other.transform.position - transform.position).normalized * knockback, DamageType.Ground);
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            active = circuit.Powered;
        else
        {
            remFrames--;
            if (remFrames <= 0)
            {
                if (active)
                    remFrames = framesDown;
                else
                    remFrames = framesUp;
                active = !active;
                
            }
        }
        GetComponent<Collider2D>().enabled = active;
        animator.SetBool("active", active);
        
    }
}
