using System;
using System.Collections.Generic;
using UnityEngine;

public class DesireAI : MonoBehaviour
{
    [Serializable]
    public class Desire
    {
        public string name;
        public AIBehavior behavior;
        [ReadOnly]
        public float value;
    }

    public List<Desire> desires = new List<Desire>();
    public Transform target;

    [ReadOnly]
    public Desire currentState;

    void Start()
    {
        SetCurrentState(null);
    }

    void UpdateDesires()
    {
        foreach (Desire desire in desires)
            UpdateDesire(desire);
    }
    
    // Must override
    protected virtual void UpdateDesire(Desire desire)
    {

    }

    void ChooseBehavior()
    {
        if (currentState != null && currentState.behavior != null && !currentState.behavior.CanRelinquishControl())
            return;
        Desire choice = null;
        foreach (Desire ai in desires)
        {
            if (choice == null || choice.value < ai.value)
                choice = ai;
        }
        SetCurrentState(choice);
    }

    void SetCurrentState(Desire state)
    {
        currentState = state;
        foreach (Desire ai in desires)
        {
            if (ai.behavior == null)
            {
                Debug.LogWarning("Null behavior: " + ai.name + " on object " + gameObject.name);
                continue;
            }
            ai.behavior.enabled = (ai == state);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!LevelEditor.main.currentRoom.Contains(transform.position.ToGrid()))
        {
            SetCurrentState(null);
            return;
        }
        UpdateDesires();
        ChooseBehavior();
    }
}
