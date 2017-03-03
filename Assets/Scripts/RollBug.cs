using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollBug : MonoBehaviour
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
    RollAttack attack;
    Rigidbody2D rb2d;

    void Start()
    {
        wander = GetComponent<Wander>();
        attack = GetComponent<RollAttack>();
        rb2d = GetComponent<Rigidbody2D>();
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
        remFrames = decisionInterval;
        SetCurrentState(currentState);
    }

    void FixedUpdate()
    {
        if (!wander.enabled && !attack.enabled)
            PickRandomState();
        else if (remFrames-- <= 0)
            PickRandomState();
        transform.localRotation = Quaternion.LookRotation(Vector3.forward, rb2d.velocity.normalized);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || currentState == AIState.Attack)
            return;
        SetCurrentState(AIState.Attack);
        attack.direction = other.transform.position;
    }

}
