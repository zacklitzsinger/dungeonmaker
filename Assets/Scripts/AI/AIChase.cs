using System.Collections.Generic;
using UnityEngine;

public class AIChase : AIBehavior
{
    public float acceleration;
    [Tooltip("How close the ai has to get to a path point to consider it complete")]
    public float pathLeniancy = 0.5f;
    public Transform Target { get { return GetComponent<DesireAI>().target; } }

    public int framesBetweenPathRecalc = 30;
    List<Vector2> path;
    private int framesUntilRecalcPath;

    public LayerMask mask;

    Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void RecalcPath()
    {
        if (framesUntilRecalcPath > 0 && path != null)
            return;
        framesUntilRecalcPath = framesBetweenPathRecalc;
        path = LevelEditor.main.navcalc.CalculatePath(transform.position, Target.position);
    }

    // If the path is overcomplicated (i.e., we can reach a point on the path very directly) then skip parts of the path.
    void SimplifyPath()
    {
        if (path == null || path.Count == 0)
            return;
        while (path.Count >= 2)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, path[1], mask);
            if (hits != null && hits.Length > 0)
                return;
            path.RemoveAt(0);
        }
    }

    void FixedUpdate()
    {
        framesUntilRecalcPath = Mathf.Max(0, framesUntilRecalcPath - 1);
        if (!Target)
        {
            Debug.LogWarning("Chase with no target set!");
            return;
        }
        Debug.DrawLine(transform.position, Target.transform.position, Color.yellow);
        if (path == null || path.Count == 0 || Vector2.Distance(path[path.Count - 1], Target.position) > 1)
            RecalcPath();
        if (path == null || path.Count == 0)
            return;
        //SimplifyPath();
        // TODO: Do some movement prediction
        Debug.DrawLine(transform.position, path[0], Color.blue);
        Vector2 delta = path[0] - (Vector2)transform.position;
        // Close enough to point
        if (delta.magnitude <= pathLeniancy)
        {
            path.RemoveAt(0);
            return;
        }
        Vector2 targetDirection = delta.normalized;
        rb2d.AddForce(targetDirection * acceleration);

        transform.localRotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
    }
}
