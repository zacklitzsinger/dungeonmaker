using UnityEngine;

public class PermanentGate : MonoBehaviour
{

    Circuit circuit;
    bool on = false;

    void Update()
    {
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
            if (circuit != null)
                SetupCircuit();
        }
    }

    void FixedUpdate()
    {
        if (circuit != null && circuit.Powered)
            on = true;
    }

    void SetupCircuit()
    {
        circuit.powerConditions.Add(() => { return on; });
    }
}
