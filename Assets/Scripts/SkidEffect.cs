using UnityEngine;

public class SkidEffect : MonoBehaviour {

    public float threshold = 1;
    public float factor = 1;
    Vector2 lastVelocity;

    public ParticleSystem effectParticleSystem;
    Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponentInParent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 currentVelocity = rb2d.velocity;
        float dot = Vector2.Dot(lastVelocity.normalized, currentVelocity.normalized);
        if (threshold < Mathf.Abs(lastVelocity.magnitude - currentVelocity.magnitude * dot))
        {
            float amount = Mathf.Abs(lastVelocity.magnitude - currentVelocity.magnitude * dot);
            amount *= factor;
            effectParticleSystem.Emit(Mathf.FloorToInt(amount));
        }
        lastVelocity = currentVelocity;
    }
}
