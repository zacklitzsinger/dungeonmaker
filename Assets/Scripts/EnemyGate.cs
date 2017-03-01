using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGate : MonoBehaviour {

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
        circuit.gateConditions.Add(() => { return AllEnemiesDeadInRoom(); });
    }

    bool AllEnemiesDeadInRoom()
    {
        foreach (Vector2 node in LevelEditor.main.currentRoom)
        {
            if (!LevelEditor.main.tilemap.ContainsKey(node))
                continue;
            List<ObjectData> goList = LevelEditor.main.tilemap[node];
            if (goList == null)
                continue;
            foreach (ObjectData info in goList)
                if (info.type == ObjectType.Enemy && info.gameObject.activeInHierarchy)
                    return false;
        }
        return true;
    }
}
