using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xor : MonoBehaviour {

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
        circuit.conditions.Add(() => { return circuit.powerAmount == 1; });
    }
}
