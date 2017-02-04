using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelInfo
{
    public string name;
    public Dictionary<Vector2, List<ObjectInfo>> tilemap = new Dictionary<Vector2, List<ObjectInfo>>();
}
