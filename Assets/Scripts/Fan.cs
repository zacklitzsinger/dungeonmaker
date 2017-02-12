using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour {
    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
        {
            Wind wind = GetComponentInChildren<Wind>();
            wind.active = circuit.Powered;
        }
    }
}
