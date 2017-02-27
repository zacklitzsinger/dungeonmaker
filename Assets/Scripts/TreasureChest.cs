﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TreasureChest : MonoBehaviour, ICustomSerializable
{

    public enum ChestItem
    {
        None,
        Key,
        GustItem
    }

    [PlayerEditableEnum("Contents", typeof(ChestItem))]
    public ChestItem contents;

    public GameObject[] prefabOptions;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        Destroy(gameObject);
        if (contents == ChestItem.None)
            return;
        if (contents == ChestItem.Key)
        {
            other.GetComponentInParent<Player>().keys++;
            return;
        }
        GameObject prefab = Array.Find(prefabOptions, (p) => { return p.name == contents.ToString(); });
        if (!prefab)
        {
            Debug.LogWarning("Could not find prefab on treasure chest: " + contents.ToString());
            return;
        }
        GameObject go = Instantiate(prefab, other.transform);
        go.transform.localPosition = Vector3.zero;
        go.name = prefab.name;
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
