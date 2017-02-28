using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extensions {

    public static Vector2 ToGrid(this Vector2 pos)
    {
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        return pos;
    }

    public static Vector2 ToGrid(this Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        return pos;
    }
}
