using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour {


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;
        Destroy(gameObject);
        // TODO: This is a bad way to do this...
        other.gameObject.AddComponent<GustItem>();
    }

}
