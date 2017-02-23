﻿using System;
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
    Interactable,
    Enemy
}

public class ObjectData : MonoBehaviour, ICustomSerializable
{
    public Guid guid; // Set outside of this behavior...
    public ObjectType type;
    public bool seeThrough = true;
    /// <summary>
    /// Counts as ground for the purposes of falling.
    /// </summary>
    public bool ground = false;
    public bool hideInPlayMode = false;
    SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        UpdateSortOrder();
    }

    void Update()
    {
        if (hideInPlayMode)
        {
            sprite.enabled = (LevelEditor.main.mode >= EditMode.Create);
        }
        UpdateSortOrder();
    }

    void UpdateSortOrder()
    {
        // Sprite draw order is dependent on Y because we are 2.5D. However, we multiple everything by a constant so that 
        // we don't get jumps when rounding. Ties are broken by object type on layer.
        sprite.sortingOrder = -5 * (int)Math.Floor(transform.position.y * 5) + (int)type * 5;
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


