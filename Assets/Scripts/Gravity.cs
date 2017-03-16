using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles objects that should touching floor or else fall.
/// </summary>
public class Gravity : MonoBehaviour {

    [ReadOnly]
    public HashSet<Collider2D> touching = new HashSet<Collider2D>();

    float baseDrag = 1f;
    public float dragModifier = 1;

    IDamageable health;
    Rigidbody2D rb2d;

    void Start()
    {
        // Currently assumes the object will have health - not necessarily a valid assumption
        health = GetComponentInParent<IDamageable>();
        rb2d = GetComponentInParent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ObjectData data = other.GetComponent<ObjectData>();
        if (data && data.ground)
            touching.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        ObjectData data = other.GetComponent<ObjectData>();
        if (data && data.ground)
            touching.Remove(other);
        CheckForDeath();
    }

    void CheckForDeath()
    {
        if (touching.Count <= 0)
            health.Damage(1, gameObject, Vector2.zero, DamageType.Fall);
    }

    void FixedUpdate()
    {
        GameObject floor = LevelEditor.main.GetGameObjectAtPointWithType(transform.position, ObjectType.Floor);
        FloorData floorData = null;
        if (floor != null)
        {
            floorData = floor.GetComponent<FloorData>();
            baseDrag = floorData.linearDrag;
        }
        rb2d.drag = baseDrag * dragModifier;
    }
}
