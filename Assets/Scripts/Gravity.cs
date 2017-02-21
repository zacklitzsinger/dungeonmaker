using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles objects that should touching floor or else fall.
/// </summary>
public class Gravity : MonoBehaviour {

    [ReadOnly]
    public List<Collider2D> touching = new List<Collider2D>();
    Health health;

    void Start()
    {
        // Currently assumes the object will have health - not necessarily a valid assumption
        health = GetComponent<Health>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData data = other.GetComponent<ObjectData>();
        if (data && (data.type == ObjectType.Floor || data.ground) && !touching.Contains(other))
            touching.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        ObjectData data = other.GetComponent<ObjectData>();
        if (data && (data.type == ObjectType.Floor || data.ground))
            touching.Remove(other);
        CheckForDeath();
    }

    void CheckForDeath()
    {
        if (touching.Count <= 0)
        {
            health.Damage(health.currentHealth);
        }
    }
}
