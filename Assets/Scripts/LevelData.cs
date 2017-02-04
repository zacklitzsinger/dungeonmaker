using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public string name;
    public Dictionary<Vector2, string> tilemap = new Dictionary<Vector2, string>();
}
