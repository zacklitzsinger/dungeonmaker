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


