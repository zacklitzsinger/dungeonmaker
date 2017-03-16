using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    Collider2D collider2d;
    SpriteRenderer spriteRenderer;
    Circuit circuit;

    void Start()
    {
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetupCircuit();
    }

    void FixedUpdate()
    {
        SetupCircuit();
        if (circuit)
        {
            collider2d.enabled = circuit.Powered;
            spriteRenderer.enabled = circuit.Powered;
        }
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player") || other.isTrigger)
            return;

        Player player = other.GetComponentInParent<Player>();
        player.actions.Add(new Player.Action() { type = Player.State.Victory });
        LevelEditor.main.ChangeMode(EditMode.Victory);
        Destroy(gameObject);
    }
}
