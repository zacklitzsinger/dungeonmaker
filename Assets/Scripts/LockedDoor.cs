using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : MonoBehaviour {

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            var player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player.keys > 0)
            {
                player.keys--;
                Destroy(gameObject);
            }
        }
    }
}
