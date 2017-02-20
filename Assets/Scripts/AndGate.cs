using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndGate : MonoBehaviour {

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
        circuit.gateConditions.Add(() => { return circuit.powerAmount >= circuit.inputs.Count; });
    }
}
