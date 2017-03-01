using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{

    public enum AIState
    {
        Idle,
        Wander
    }

    [ReadOnly]
    public AIState currentState = AIState.Wander;
    public int decisionInterval = 60;
    int remFrames;

    Wander wander;

    void Start()
    {
        wander = GetComponent<Wander>();
    }

    void FixedUpdate()
    {
        if (remFrames-- <= 0)
        {
            currentState = (AIState)Random.Range(0, System.Enum.GetNames(typeof(AIState)).Length);
            remFrames = decisionInterval;
            if (currentState == AIState.Idle)
                wander.enabled = false;
            else
                wander.enabled = true;
        }
    }
}
