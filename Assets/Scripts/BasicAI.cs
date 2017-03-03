using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicAI : MonoBehaviour
{
    [ReadOnly]
    public MonoBehaviour currentState;
    [Tooltip("How frequently to make decisions between random states")]
    public int decisionInterval = 60;
    int remFrames;
    [Tooltip("How many frames to stagger per instance of damage")]
    public int hitStagger;

    public List<MonoBehaviour> randomStates = new List<MonoBehaviour>();
    public MonoBehaviour attack;

    Circuit circuit;
    VisionCone vision;
    Rigidbody2D rb2d;
    Health health;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        vision = GetComponent<VisionCone>();
        health = GetComponent<Health>();
        // When damaged by the player, they should become the focus. This code can't be in the behaviors
        // because they are in various states of enabled/disabled.
        health.onDamaged += (go) =>
        {
            if (go.CompareTag("Player") && vision)
                vision.target = go.transform;
            if (attack as KnightAttack)
                (attack as KnightAttack).staggerFrames += hitStagger;
        };
    }

    void SetCurrentState(MonoBehaviour state)
    {
        foreach (MonoBehaviour behavior in randomStates)
            behavior.enabled = (state == behavior);
        if (attack)
            attack.enabled = (attack == state);
    }

    void PickRandomState()
    {
        // Don't randomly pick attack state
        currentState = randomStates[UnityEngine.Random.Range(0, randomStates.Count)];
        SetCurrentState(currentState);
        remFrames = decisionInterval;
    }

    void FixedUpdate()
    {
        if (!LevelEditor.main.currentRoom.Contains(transform.position.ToGrid()))
            SetCurrentState(null);
        if (circuit == null)
            circuit = GetComponent<Circuit>();
        if (circuit)
        {
            if (!circuit.Powered)
            {
                health.invulnerableOverride = true;
                SetCurrentState(null);
            }
            else
            {
                health.invulnerableOverride = false;
            }
        }
        if (vision && vision.target != null)
            SetCurrentState(attack);
        else if (!randomStates.Exists((s) => { return s && s.enabled; }) && (!attack || !attack.enabled) || remFrames-- <= 0)
            PickRandomState();
        if (vision && vision.target != null && (vision.alwaysTrackPlayer || randomStates.Exists((s) => { return s && s.enabled; })))
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, (vision.target.position - transform.position).normalized);
        else if (rb2d.velocity.magnitude > 0)
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, rb2d.velocity.normalized);
    }
}
