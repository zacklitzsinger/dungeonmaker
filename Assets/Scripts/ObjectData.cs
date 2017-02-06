using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Floor,
    FloorAttachment, // E.g., spikes, grass, etc.
    Wall,
    Interactable
}

[Serializable]
public class ObjectInfo
{
    public string name;
    public ObjectType type;
}

public class ObjectData : MonoBehaviour
{
    public ObjectInfo info;
}


