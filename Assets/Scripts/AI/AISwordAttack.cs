using System;
using System.Collections.Generic;
using UnityEngine;

public class AISwordAttack : AIBehavior
{
    [Serializable]
    public class Action
    {
        public State type;
        public int frames;
        public Vector2 direction;
    }

    public int minAttackDelay;
    public int maxAttackDelay;
    public int attackFrames;
    public float attackMoveForce;
    public int postAttackDelay;
    public GameObject sword;
    public Transform Target { get { return GetComponent<DesireAI>().target; } }
    [ReadOnly]
    public int combo;
    [Range(0, 1)]
    public float comboChance;
    public int maxCombo;

    public enum State
    {
        Idle,
        AttackWindup,
        Attack
    }

    public Action currentAction;
    public Queue<Action> actions = new Queue<Action>();

    Rigidbody2D rb2d;

    public override bool CanRelinquishControl()
    {
        return currentAction == null && actions.Count == 0;
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        combo = 0;
        actions.Enqueue(new Action() { type = State.AttackWindup, frames = UnityEngine.Random.Range(minAttackDelay, maxAttackDelay) });
        actions.Enqueue(new Action() { type = State.Attack, frames = attackFrames, direction = (Target.position - transform.position) });
    }

    public void TriggerAction(Action action)
    {
        switch (action.type)
        {
            case State.Attack:
                combo++;
                Quaternion rotation = Quaternion.LookRotation(UnityEngine.Random.value < 0.5f ? Vector3.forward : Vector3.back, action.direction.normalized);
                Sword swordInstance = Instantiate(sword, transform.position, rotation, transform).GetComponentInChildren<Sword>();
                swordInstance.friendly = false;
                swordInstance.owner = gameObject;
                rb2d.AddForce(action.direction * attackMoveForce);
                if (combo < maxCombo && UnityEngine.Random.value < comboChance)
                    actions.Enqueue(new Action() { type = State.Attack, frames = attackFrames, direction = (Target.position - transform.position) });
                else
                    // After attacking, force a waiting period
                    actions.Enqueue(new Action() { type = State.Idle, frames = postAttackDelay });
                break;
        }
    }

    void FixedUpdate()
    {
        if (!Target)
        {
            Debug.LogWarning("No target for attack!");
            return;
        }
        Debug.DrawLine(transform.position, Target.transform.position, Color.yellow);
        if (currentAction == null || currentAction.frames-- <= 0)
        {
            if (actions.Count > 0)
            {
                currentAction = actions.Dequeue();
                TriggerAction(currentAction);
            }
            else
            {
                currentAction = null;
                enabled = false;
            }
        }
        transform.localRotation = Quaternion.LookRotation(Vector3.forward, (Target.transform.position - transform.position).normalized);
    }
}
