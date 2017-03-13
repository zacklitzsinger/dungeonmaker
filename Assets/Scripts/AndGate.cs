using UnityEngine;

public class AndGate : MonoBehaviour
{

    Circuit circuit;

    void Start()
    {
        SetupCircuit();
    }

    void Update()
    {
        SetupCircuit();
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
        if (circuit == null)
            return;
        circuit.gateConditions.Add(() =>
        {
            return circuit.powerAmount >= circuit.inputs.Count;
        });
    }
}
