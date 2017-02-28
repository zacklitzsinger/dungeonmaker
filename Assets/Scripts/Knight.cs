using UnityEngine;

public class Knight: MonoBehaviour
{

    public enum AIState
    {
        Idle,
        Wander,
        Attack
    }

    [ReadOnly]
    public AIState currentState = AIState.Wander;
    public int decisionInterval = 60;
    int remFrames;

    Wander wander;
    VisionCone vision;
    KnightAttack attack;
    Rigidbody2D rb2d;
    Health health;

    void Start()
    {
        wander = GetComponent<Wander>();
        attack = GetComponent<KnightAttack>();
        rb2d = GetComponent<Rigidbody2D>();
        vision = GetComponent<VisionCone>();
        health = GetComponent<Health>();
        // When damaged by the player, they should become the focus. This code can't be in the behaviors
        // because they are in various states of enabled/disabled.
        health.onDamaged += (go) =>
        {
            if (go.CompareTag("Player"))
                vision.target = go.transform;
            attack.staggerFrames += 15;
        };
    }

    void SetCurrentState(AIState state)
    {
        currentState = state;
        wander.enabled = (currentState == AIState.Wander);
        attack.enabled = (currentState == AIState.Attack);
    }

    void PickRandomState()
    {
        // Don't randomly pick attack state
        currentState = (AIState)Random.Range(0, System.Enum.GetNames(typeof(AIState)).Length - 1);
        SetCurrentState(currentState);
        remFrames = decisionInterval;
    }

    void FixedUpdate()
    {
        if (vision.target != null)
            SetCurrentState(AIState.Attack);
        else if (!wander.enabled && !attack.enabled)
            PickRandomState();
        else if (remFrames-- <= 0)
            PickRandomState();
        if (vision.target != null)
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, (vision.target.position - transform.position).normalized);
        else if (rb2d.velocity.magnitude > 0)
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, rb2d.velocity.normalized);
    }
}
