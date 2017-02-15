using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour {

    public List<MapNode> path = new List<MapNode>();
    List<MapNode> choices;
    public float acceleration;

    NavigationCalculator<MapNode> navcalc;
    Rigidbody2D rb2d;

    enum AIState
    {
        Idle,
        Wander
    }

    AIState currentState = AIState.Wander;
    int decisionInterval = 60;
    int remFrames;

    void Start()
    {
        navcalc = LevelEditor.main.navcalc;
        rb2d = GetComponent<Rigidbody2D>();
    }

	void FixedUpdate () {
        if (choices == null)
            choices = navcalc.GetConnectedNodes(GetCurrentNode());
        if (remFrames-- <= 0)
        {
            currentState = (AIState)Random.Range(0, System.Enum.GetNames(typeof(AIState)).Length);
            remFrames = decisionInterval;
        }

        switch (currentState)
        {
            case AIState.Idle:
                break;
            case AIState.Wander:
                if (path == null || path.Count == 0)
                    ChooseTarget();
                if (path != null && path.Count > 0)
                {
                    Vector2 delta = path[0].ToVector2() - (Vector2)transform.position;
                    if (delta.magnitude <= 0.5f)
                    {
                        path.RemoveAt(0);
                        return;
                    }
                    Vector2 targetDirection = delta.normalized;
                    rb2d.AddForce(targetDirection * acceleration);
                }
                break;
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
        MapNode target = choices[Random.Range(0, choices.Count)];
        path = navcalc.CalculatePath(currentNode, target);
        if (path == null)
            Debug.LogWarning("Could not find path!");
    }
}
