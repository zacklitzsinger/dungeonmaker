using System.Collections.Generic;
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
    public int attackFrames;
    public float attackDistance;
    public GameObject sword;

    public enum State
    {
        Idle,
        Chase,
        Attack,
    }

    public KnightAction currentAction;
    public Queue<KnightAction> actions = new Queue<KnightAction>();
    List<MapNode> path;

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
                Sword swordInstance = Instantiate(sword, transform.position, Quaternion.LookRotation(Vector3.forward, action.direction.normalized), transform).GetComponentInChildren<Sword>();
                swordInstance.friendly = false;
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
                actions.Enqueue(new KnightAction() { type = State.Attack, frames = attackFrames, direction = (vision.target.position - transform.position) });
            else
            {
                path = LevelEditor.main.navcalc.CalculatePath(LevelEditor.main.navmap.GetNode(transform.position), LevelEditor.main.navmap.GetNode(vision.target.position));
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

    void FixedUpdate()
    {
        if (vision.target)
            Debug.DrawLine(transform.position, vision.target.transform.position, Color.yellow);
        if (currentAction.type == State.Chase && path != null && path.Count > 0)
        {
            Debug.DrawLine(transform.position, path[0].ToVector2(), Color.blue);
            Vector2 delta = path[0].ToVector2() - (Vector2)transform.position;
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
