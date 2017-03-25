using UnityEngine;

public class Key : MonoBehaviour {

    public AudioClip sound;

    Collider2D collider2d;
    MeshRenderer meshRenderer;
    Circuit circuit;

    void Start()
    {
        collider2d = GetComponent<Collider2D>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        SetupCircuit();
    }

    void FixedUpdate()
    {
        SetupCircuit();
        if (circuit)
        {
            collider2d.enabled = circuit.Powered;
            meshRenderer.gameObject.SetActive(circuit.Powered);
        }
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
        if (circuit == null)
            return;
        circuit.gateConditions.Add(() => { return !enabled; });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            enabled = false;
            collider2d.enabled = false;
            meshRenderer.gameObject.SetActive(false);
            other.GetComponentInParent<Player>().keys++;
            AudioSource.PlayClipAtPoint(sound, transform.position);
        }
    }

}
