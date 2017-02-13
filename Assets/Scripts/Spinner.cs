using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {

    [PlayerEditable("Invert")]
    public bool invert = false;
    public bool active = false;
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
        if (other.GetComponent<Wind>())
        {
            active = true;
            counter++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Wind>())
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
        circuit.conditions.Add(() => { return active ^ invert; });
    }

    void FixedUpdate()
    {
        if (active)
            rb2d.AddTorque(rotationForce);
    }

}
