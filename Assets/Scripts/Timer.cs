using System.IO;
using UnityEngine;

public class Timer : MonoBehaviour, ICustomSerializable {

    [PlayerEditableRange("Off Time", 1, 20)]
    public int offSeconds = 1;
    [PlayerEditableRange("On Time", 1, 20)]
    public int onSeconds = 1;
    [PlayerEditableRange("Offset Time", 0, 20)]
    public int offset = 0;
    public bool on;

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
        if (circuit == null)
        {
            circuit = GetComponent<Circuit>();
            if (circuit == null)
                return;
        }
        circuit.gateConditions.Add(() => { return on; });
    }

    void FixedUpdate()
    {
        float time = (Time.fixedTime + offset) % (offSeconds + onSeconds);
        on = (time > offSeconds);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

}
