using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XorGate : MonoBehaviour {

    Circuit circuit;

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
        circuit.gateConditions.Add(() => { return circuit.powerAmount == 1; });
    }
}
