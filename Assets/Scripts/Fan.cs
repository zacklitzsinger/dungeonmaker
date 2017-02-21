using System.IO;
using UnityEngine;

public class Fan : MonoBehaviour, ICustomSerializable
{

    public bool active;
    [PlayerEditableRange("Range", 1, 10)]
    public int distance;
    [PlayerEditableRange("Activation Speed", 1, 5)]
    public int activationSpeed = 2;
    public float force;

    Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        animator.SetBool("active", active);
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            active = circuit.Powered;
        Wind wind = GetComponentInChildren<Wind>();
        wind.active = active;
        wind.force = force;
        int distance = FindDistance();
        if (distance <= wind.size)
            wind.size = distance;
        else
            wind.size = Mathf.Min(wind.size + 0.016f * activationSpeed, distance); // ~ 2 tiles per second by default
    }

    int FindDistance()
    {
        if (!active)
            return 0;
        for (int i = 1; i <= distance; i++)
        {
            Vector2 pos = transform.position + transform.up * i;
            if (CheckForCollisions(LevelEditor.main.ConvertPositionToGrid(pos)))
                return i - 1;
        }
        return distance;
    }

    bool CheckForCollisions(Vector2 pos)
    {
        if (LevelEditor.main.tilemap.ContainsKey(pos))
        {
            foreach (GameObject go in LevelEditor.main.tilemap[pos])
                if (go != null && go.GetComponent<ObjectData>().type == ObjectType.Wall && go.GetComponent<Collider2D>().enabled)
                    return true;
        }
        return false;
    }

    public void Serialize(BinaryWriter bw)
    {
        ObjectSerializer.Serialize(bw, this);
    }

    public void Deserialize(BinaryReader br)
    {
        ObjectSerializer.Deserialize(br, this);
    }
}
