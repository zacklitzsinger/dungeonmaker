using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {

    [PlayerEditable("Invert")]
    public bool invert = false;
    public bool active = false;
    public float activationSpeed;
    [ReadOnly]
    public int counter = 0;
    public float rotationForce = 1f;

    Rigidbody2D rb2d;
    Circuit circuit;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<Wind>())
            return;
        if (other.GetComponent<Rigidbody2D>())
            return;
        active = true;
        counter++;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.GetComponent<Wind>())
            return;
        Rigidbody2D otherRb2d = other.GetComponent<Rigidbody2D>();
        if (!otherRb2d)
            return;
        rb2d.AddTorque(-otherRb2d.velocity.magnitude);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<Wind>())
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
            circuit = GetComponent<Circuit>();
            if (circuit != null)
                SetupCircuit();
        }
    }

    void SetupCircuit()
    {
        circuit.conditions.Add(() => { return (Mathf.Abs(rb2d.angularVelocity) > activationSpeed) ^ invert; });
    }

    void FixedUpdate()
    {
        if (active)
            rb2d.AddTorque(rotationForce);
    }

}
