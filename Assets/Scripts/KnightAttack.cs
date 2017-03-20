using System;
using System.Collections.Generic;
using UnityEngine;

public class KnightAttack : MonoBehaviour, IActionQueue, IAttack
{
    [Serializable]
    public class Action
    {
        public State type;
        public int frames;
        public Vector2 direction;
    }

    public float acceleration;
    public int minAttackDelay;
    public int maxAttackDelay;
    public int attackFrames;
    public float attackDistance;
    public float attackMoveForce;
    public int postAttackDelay;
    public GameObject sword;
    public Transform target;

    public enum State
    {
        Idle,
        Chase,
        AttackWindup,
        Attack
    }

    public Action currentAction;
    public Queue<Action> actions = new Queue<Action>();
    public int framesBetweenPathRecalc = 30;
    List<Vector2> path;
    private int framesUntilRecalcPath;

    VisionCone vision;
    Rigidbody2D rb2d;
    Shield shield;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        vision = GetComponent<VisionCone>();
        shield = GetComponentInChildren<Shield>(true);
    }

    public void TriggerAction(Action action)
    {
        if (shield)
            shield.gameObject.SetActive(action.type != State.Idle || action.frames == 0);
        switch (action.type)
        {
            case State.Attack:
                if (shield)
                    shield.gameObject.SetActive(false);
                Quaternion rotation = Quaternion.LookRotation(UnityEngine.Random.value < 0.5f ? Vector3.forward : Vector3.back, action.direction.normalized);
                Sword swordInstance = Instantiate(sword, transform.position, rotation, transform).GetComponentInChildren<Sword>();
                swordInstance.friendly = false;
                swordInstance.owner = gameObject;
                rb2d.AddForce(action.direction * attackMoveForce);
                // After attacking, force a waiting period
                actions.Enqueue(new Action() { type = State.Idle, frames = postAttackDelay });
                break;
        }
    }

    public void Interrupt(int frames)
    {
        currentAction = new Action() { type = KnightAttack.State.Idle, frames = frames };
        TriggerAction(currentAction);
    }

    Action DecideNextAction()
    {
        if (target)
        {
            if (sword && (target.position - transform.position).magnitude < attackDistance)
            {
                actions.Enqueue(new Action() { type = State.AttackWindup, frames = UnityEngine.Random.Range(minAttackDelay, maxAttackDelay) });
                actions.Enqueue(new Action() { type = State.Attack, frames = attackFrames, direction = (target.position - transform.position) });
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
                    actions.Enqueue(new Action() { type = State.Chase });
            }
        }
        else
            enabled = false;
        return currentAction = new Action();
    }

    void RecalcPath()
    {
        if (framesUntilRecalcPath > 0)
            return;
        framesUntilRecalcPath = framesBetweenPathRecalc;
        path = LevelEditor.main.navcalc.CalculatePath(transform.position, target.position);
    }

    void FixedUpdate()
    {
        framesUntilRecalcPath = Mathf.Max(0, framesUntilRecalcPath - 1);
        if (vision)
            target = target ?? vision.target;
        if (target)
            Debug.DrawLine(transform.position, target.transform.position, Color.yellow);
        if (currentAction != null && currentAction.type == State.Chase && path != null && path.Count > 0)
        {
            if (target && (target.position - transform.position).magnitude < attackDistance)
            {
                TriggerAction(DecideNextAction());
                return;
            }
            // TODO: Do some movement prediction
            if (target != null && Vector2.Distance(path[path.Count - 1], target.position) > 1)
                RecalcPath();
            if (path.Count > 0)
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
        else if (currentAction == null || currentAction.frames-- <= 0)
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

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

}
