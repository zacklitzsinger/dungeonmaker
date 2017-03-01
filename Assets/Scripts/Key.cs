using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour {

    public AudioClip sound;

    Collider2D collider2d;
    SpriteRenderer spriteRenderer;
    Circuit circuit;

    void Start()
    {
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
            if (circuit)
                circuit.gateConditions.Add(() => { return !enabled; });
        }
        if (circuit)
        {
            collider2d.enabled = circuit.Powered;
            spriteRenderer.enabled = circuit.Powered;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enabled = false;
            collider2d.enabled = false;
            spriteRenderer.enabled = false;
            other.GetComponentInParent<Player>().keys++;
            AudioSource.PlayClipAtPoint(sound, transform.position);
        }
    }

}
