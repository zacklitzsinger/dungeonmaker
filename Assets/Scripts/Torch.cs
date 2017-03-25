using UnityEngine;

public class Torch : MonoBehaviour {

    LightFlicker lickFlicker;
    ParticleSystem ps;
    Circuit circuit;

    void Start()
    {
        lickFlicker = GetComponentInChildren<LightFlicker>();
        ps = GetComponentInChildren<ParticleSystem>();
        SetupCircuit();
    }

    void FixedUpdate()
    {
        SetupCircuit();
        if (circuit)
        {
            lickFlicker.active = circuit.Powered;
            if (circuit.Powered)
                ps.Play(true);
            else
                ps.Stop(true);
        }
    }

    void SetupCircuit()
    {
        if (circuit != null)
            return;
        circuit = GetComponent<Circuit>();
    }
}
