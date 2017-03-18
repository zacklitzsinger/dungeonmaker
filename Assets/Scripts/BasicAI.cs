using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicAI : MonoBehaviour
{
    public MonoBehaviour previousState;
    [ReadOnly]
    public MonoBehaviour currentState;
    [Tooltip("How frequently to make decisions between random states")]
    public int decisionInterval = 60;
    int remFrames;
    [Tooltip("How many frames to stagger per instance of damage")]
    public int hitStagger;
    [ReadOnly]
    public int staggerFrames;

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
        health.onDamaged += (go) =>
        {
            staggerFrames += hitStagger;
        };
    }

    public void SetCurrentState(MonoBehaviour state)
    {
        if (currentState != state)
        {
            previousState = currentState;
            currentState = state;
        }
        foreach (MonoBehaviour behavior in randomStates)
            behavior.enabled = (state == behavior);
        if (attack)
            attack.enabled = (attack == state);
    }

    void PickRandomState()
    {
        SetCurrentState(randomStates[UnityEngine.Random.Range(0, randomStates.Count)]);
        remFrames = decisionInterval;
    }

    void FixedUpdate()
    {
        if (!LevelEditor.main.currentRoom.Contains(transform.position.ToGrid()))
        {
            SetCurrentState(null);
            previousState = null;
            return;
        }
        if (circuit == null)
            circuit = GetComponent<Circuit>();
        if (circuit)
        {
            if (!circuit.Powered)
            {
                health.invulnerableOverride = true;
                SetCurrentState(null);
                previousState = null;
            }
            else
            {
                health.invulnerableOverride = false;
            }
        }
        if (staggerFrames > 0)
        {
            SetCurrentState(null);
            staggerFrames--;
            return;
        }
        else if (staggerFrames == 0 && currentState == null)
        {
            SetCurrentState(previousState);
        }
        if (vision && vision.target != null)
            SetCurrentState(attack);
        else if ((!randomStates.Exists((s) => { return s && s.enabled; }) || remFrames-- <= 0) && (currentState == null || randomStates.Contains(currentState)) )
            PickRandomState();
        if (vision && vision.target != null && (vision.alwaysTrackPlayer || randomStates.Exists((s) => { return s && s.enabled; })))
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, (vision.target.position - transform.position).normalized);
        else if (rb2d.velocity.magnitude > 0)
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, rb2d.velocity.normalized);
    }
}
