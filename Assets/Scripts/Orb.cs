using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        Player player = other.GetComponentInParent<Player>();
        player.state = PlayerState.Victory;
        LevelEditor.main.ChangeMode(EditMode.Victory);
        Destroy(gameObject);
    }
}
