using UnityEngine;

public class Spinner : MonoBehaviour {

    public bool active = false;
    public float activationSpeed;
    [ReadOnly]
    public int counter = 0;
    public float rotationForce = 1f;
    public float gustMultiplier = 4f;

    Rigidbody2D rb2d;
    Circuit circuit;

    void Start()
    {
        rb2d = GetComponentInChildren<Rigidbody2D>();
    }

    // Handle wind zones
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Wind"))
            return;
        if (other.GetComponent<Rigidbody2D>())
            return;
        active = true;
        counter++;
    }

    // Handles gusts of wind
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Wind"))
            return;
        Rigidbody2D otherRb2d = other.GetComponent<Rigidbody2D>();
        if (!otherRb2d)
            return;
        rb2d.AddTorque(-otherRb2d.velocity.magnitude * gustMultiplier);
    }

    // Handle wind zones
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Wind"))
            return;
        if (other.GetComponent<Rigidbody2D>())
            return;
        counter--;
        if (counter <= 0)
            active = false;
    }

    void Update()
    {
        if (circuit == null)
        {
            circuit = GetComponentInParent<Circuit>();
            if (circuit != null)
                SetupCircuit();
        }
    }

    void SetupCircuit()
    {
        circuit.gateConditions.Add(() => { return (Mathf.Abs(rb2d.angularVelocity) >= activationSpeed); });
    }

    void FixedUpdate()
    {
        if (active)
            rb2d.AddTorque(rotationForce);
    }

}
