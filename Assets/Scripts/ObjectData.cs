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
    SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
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
        type = (ObjectType)br.ReadInt32();
        transform.Rotate(Vector3.forward * br.ReadSingle());
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write((int)type);
        bw.Write(transform.rotation.eulerAngles.z);
    }
}


