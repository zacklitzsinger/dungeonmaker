using UnityEngine;

public class KnightAI : DesireAI
{
    public float attackRange;

    Health health;
    VisionCone vision;

    void Start()
    {
        vision = GetComponent<VisionCone>();
        health = GetComponent<Health>();
        health.onDamaged += (source) =>
        {
            target = source.transform;
        };
    }

    protected override void UpdateDesire(Desire desire)
    {
        switch (desire.state)
        {
            case State.Wander:
                desire.value = 0.1f;
                break;
            case State.Chase:
                if (target)
                    desire.value = Vector2.Distance(transform.position, target.position) > attackRange ? 0.5f : 0f;
                else
                    desire.value = 0;
                break;
            case State.Attack:
                if (target)
                    desire.value = Vector2.Distance(transform.position, target.position) <= attackRange ? 0.5f : 0f;
                else
                    desire.value = 0;
                break;
            default:
                Debug.LogWarning("Unhandled desire: " + desire);
                break;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (vision && vision.target && target == null)
            target = vision.target;
    }
}
