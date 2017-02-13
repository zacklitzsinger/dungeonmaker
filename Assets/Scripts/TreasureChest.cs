using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour {

    public GameObject prefabItem;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        Destroy(gameObject);
        GameObject go = Instantiate(prefabItem, other.transform);
        go.transform.localPosition = Vector3.zero;
        go.name = prefabItem.name;
    }

}
