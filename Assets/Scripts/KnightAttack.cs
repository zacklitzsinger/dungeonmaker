﻿using System.Collections.Generic;
using UnityEngine;

public struct KnightAction
{
    public KnightAttack.State type;
    public int frames;
    public Vector2 direction;
}

public class KnightAttack : MonoBehaviour
{

    public float acceleration;
    public int minAttackDelay;
    public int maxAttackDelay;
    public int attackFrames;
    public float attackDistance;
    public float attackMoveForce;
    public GameObject sword;
    public int staggerFrames; // can't do anything while stagger frames > 0

    public enum State
    {
        Idle,
        Chase,
        Attack,
    }

    public KnightAction currentAction;
    public Queue<KnightAction> actions = new Queue<KnightAction>();
    List<Vector2> path;

    VisionCone vision;
    Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        vision = GetComponent<VisionCone>();
    }

    void TriggerAction(KnightAction action)
    {
        switch (action.type)
        {
            case State.Attack:
                Quaternion rotation = Quaternion.LookRotation(Random.value < 0.5f ? Vector3.forward : Vector3.back, action.direction.normalized);
                Sword swordInstance = Instantiate(sword, transform.position, rotation, transform).GetComponentInChildren<Sword>();
                swordInstance.friendly = false;
                swordInstance.owner = gameObject;
                rb2d.AddForce(action.direction * attackMoveForce);
                // After attacking, force a waiting period
                actions.Enqueue(new KnightAction() { type = State.Idle, frames = 60 });
                break;
        }
    }

    KnightAction DecideNextAction()
    {
        if (vision.target)
        {
            if ((vision.target.position - transform.position).magnitude < attackDistance)
            {
                actions.Enqueue(new KnightAction() { type = State.Idle, frames = Random.Range(minAttackDelay, maxAttackDelay) });
                actions.Enqueue(new KnightAction() { type = State.Attack, frames = attackFrames, direction = (vision.target.position - transform.position) });
            }
            else
            {
                RecalcPath();
                if (path == null)
                {
                    Debug.LogWarning("failed to find path! " + gameObject.name);
                    enabled = false;
                }
                else
                    actions.Enqueue(new KnightAction() { type = State.Chase });
            }
        }
        return currentAction = new KnightAction(); 
    }

    void RecalcPath()
    {
        path = LevelEditor.main.navcalc.CalculatePath(transform.position, vision.target.position);
    }

    void FixedUpdate()
    {
        if (vision.target)
            Debug.DrawLine(transform.position, vision.target.transform.position, Color.yellow);
        if (staggerFrames > 0)
        {
            staggerFrames--;
            return;
        }
        if (currentAction.type == State.Chase && path != null && path.Count > 0)
        {
            if (vision.target && (vision.target.position - transform.position).magnitude < attackDistance)
            {
                TriggerAction(DecideNextAction());
                return;
            }
            // TODO: Do some movement prediction
            if (vision.target != null && Vector2.Distance(path[path.Count - 1], vision.target.position) > 1)
                RecalcPath();
            Debug.DrawLine(transform.position, path[0], Color.blue);
            Vector2 delta = path[0] - (Vector2)transform.position;
            // Close enough to point
            if (delta.magnitude <= 0.5f)
            {
                path.RemoveAt(0);
                return;
            }
            Vector2 targetDirection = delta.normalized;
            rb2d.AddForce(targetDirection * acceleration);
        }
        else if (currentAction.frames-- <= 0)
        {
            if (actions.Count == 0)
                actions.Enqueue(DecideNextAction());
            if (actions.Count > 0)
            {
                currentAction = actions.Dequeue();
                TriggerAction(currentAction);
            }
        }
    }

}