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
        if (currentState == AIState.Wander)
            wander.enabled = true;
        else
            wander.enabled = false;
        if (currentState == AIState.Attack)
            attack.enabled = true;
        else
            attack.enabled = false;
    }

    void PickRandomState()
    {
        // Don't randomly pick attack state
        currentState = (AIState)Random.Range(0, System.Enum.GetNames(typeof(AIState)).Length - 1);
        remFrames = decisionInterval;
        SetCurrentState(currentState);
        remFrames = decisionInterval;
    }

    void FixedUpdate()
    {
        if (!wander.enabled && !attack.enabled)
            PickRandomState();
        if (remFrames-- <= 0)
            PickRandomState();
        transform.localRotation = Quaternion.LookRotation(Vector3.forward, rb2d.velocity.normalized);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;
        int dmg = collision.collider.GetComponent<Health>().Damage(1);
        if (dmg > 0)
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            collision.rigidbody.AddForce(dir * 1200f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        SetCurrentState(AIState.Attack);
        attack.target = other.transform.position;
    }

}
