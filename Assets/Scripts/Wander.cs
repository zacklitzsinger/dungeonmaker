using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour
{

    public float acceleration;
    public bool flying = false;

    public List<Vector2> path = new List<Vector2>();
    List<Vector2> choices;
    Vector2 chosenTarget;

    NavigationCalculator navcalc;
    Rigidbody2D rb2d;


    void Start()
    {
        navcalc = LevelEditor.main.navcalc;
        rb2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (choices == null)
            choices = navcalc.GetConnectedNodes(LevelEditor.main.ConvertPositionToGrid(transform.position), flying);

        Debug.DrawLine(transform.position, chosenTarget, Color.cyan);
        if (path != null)
            for (int i = 0; i < path.Count; i++)
                Debug.DrawLine(i > 0 ? path[i - 1] : (Vector2)transform.position, path[i], Color.magenta);


        if (path == null || path.Count == 0)
            ChooseTarget();
        if (path != null && path.Count > 0)
        {
            Vector2 delta = path[0] - (Vector2)transform.position;
            // Close enough to point
            if (delta.magnitude <= 0.5f)
            {
                path.RemoveAt(0);
                return;
            }
            Vector2 targetDirection = delta.normalized;
            rb2d.AddForce(targetDirection * acceleration);
            if (path.Count == 0)
                enabled = false;
        }
    }

    void ChooseTarget()
    {
        Vector2 currentNode = LevelEditor.main.ConvertPositionToGrid(transform.position);
        if (!choices.Contains(currentNode))
            Debug.LogWarning("Changed room!");
        Vector2 target;
        do
        {
            target = choices[Random.Range(0, choices.Count)];
        } while (choices.Count > 1 && target == currentNode);
        chosenTarget = target;
        path = navcalc.CalculatePath(currentNode, target, flying);
        if (path == null)
            Debug.LogWarning("Could not find path! " + gameObject.name + ", currentNode: " + currentNode + ", target: " + target);
    }
}
