using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Fan : MonoBehaviour, ICustomSerializable
{

    public bool active;
    [PlayerEditableRange("Range", 1, 10)]
    public int distance;
    public float force;

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            active = circuit.Powered;
        GetComponentInChildren<Wind>().active = active;
        GetComponentInChildren<Wind>().size = FindDistance();
    }

    int FindDistance()
    {
        for (int i = 1; i <= distance; i++)
        {
            Vector2 pos = transform.position + transform.up * i;
            if (CheckForCollisions(LevelEditor.main.ConvertWorldPositionToGrid(pos)))
                return i-1;
        }
        return distance;
    }

    bool CheckForCollisions(Vector2 pos)
    {
        if (LevelEditor.main.tilemap.ContainsKey(pos))
        {
            foreach (GameObject go in LevelEditor.main.tilemap[pos])
                if (go.GetComponent<ObjectData>().type == ObjectType.Wall && go.GetComponent<Collider2D>().enabled)
                    return true;
        }
        return false;
    }

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }
}
