using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ObjectType
{
    None,
    Floor,
    FloorAttachment, // E.g., spikes, grass, etc.
    Wall,
    Interactable
}

public class ObjectData : MonoBehaviour, ICustomSerializable
{
    public Guid guid; // Set outside of this behavior...
    public ObjectType type;
    public bool seeThrough = true;
    /// <summary>
    /// Counts as ground for the purposes of falling even though its not a floor tile.
    /// </summary>
    public bool ground = false;
    SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        UpdateSortOrder();
    }

    void Update()
    {
        UpdateSortOrder();
    }

    void UpdateSortOrder()
    {
        // Sprite draw order is dependent on Y because we are 2.5D. However, we multiple everything by 2 so that 
        // we don't get jumps when rounding. Ties are broken by object type on layer.
        sprite.sortingOrder = -4 * (int)Math.Floor(transform.position.y * 4) + (int)type * 4;
    }

    public bool Navigable
    {
        get
        {
            return type != ObjectType.Wall;
        }
    }

    public void Deserialize(BinaryReader br)
    {
        transform.Rotate(Vector3.forward * br.ReadSingle());
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(transform.rotation.eulerAngles.z);
    }
}


