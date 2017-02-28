using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour
{

    public float acceleration;
    public bool flying = false;

    public List<MapNode> path = new List<MapNode>();
    List<MapNode> choices;
    Vector2 chosenTarget;

    NavigationCalculator<MapNode> navcalc;
    Rigidbody2D rb2d;


    void Start()
    {
        navcalc = LevelEditor.main.navcalc;
        rb2d = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (choices == null)
            choices = navcalc.GetConnectedNodes(GetCurrentNode(), flying);

        Debug.DrawLine(transform.position, chosenTarget, Color.cyan);
        if (path != null)
            for (int i = 0; i < path.Count; i++)
                Debug.DrawLine(i > 0 ? path[i - 1].ToVector2() : (Vector2)transform.position, path[i].ToVector2(), Color.magenta);


        if (path == null || path.Count == 0)
            ChooseTarget();
        if (path != null && path.Count > 0)
        {
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
    }

    MapNode GetCurrentNode()
    {
        return new MapNode(LevelEditor.main.ConvertPositionToGrid(transform.position));
    }

    void ChooseTarget()
    {
        MapNode currentNode = GetCurrentNode();
        if (!choices.Contains(currentNode))
            Debug.LogWarning("Changed room!");
        MapNode target;
        do
        {
            target = choices[Random.Range(0, choices.Count)];
        } while (choices.Count > 1 && target == currentNode);
        chosenTarget = target.ToVector2();
        path = navcalc.CalculatePath(currentNode, target, flying);
        if (path == null)
            Debug.LogWarning("Could not find path! " + gameObject.name + ", currentNode: " + currentNode + ", target: " + target);
    }
}
