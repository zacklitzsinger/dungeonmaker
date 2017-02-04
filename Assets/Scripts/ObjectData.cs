using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Floor,
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


