using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

    public AudioClip sound;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<Player>().keys++;
            AudioSource.PlayClipAtPoint(sound, transform.position);
            gameObject.SetActive(false);
        }
    }

}
