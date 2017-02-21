using System.IO;
using UnityEngine;

public class Flamethrower : MonoBehaviour, ICustomSerializable
{

    public bool active;
    [PlayerEditableRange("Range", 1, 10)]
    public int distance;
    [PlayerEditable("Invert")]
    public bool invert;

    Fire fire;
    Animator animator;

    void Start()
    {
        fire = GetComponentInChildren<Fire>();
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
            active = circuit.Powered ^ invert;
        fire.active = active;
        int distance = FindDistance();
        if (distance <= fire.size)
            fire.size = distance;
        else
            fire.size = Mathf.Min(fire.size + 0.016f, distance); // ~ 1 tiles per second
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
            foreach (GameObject go in LevelEditor.main.tilemap[pos])
                if (go != null && go.GetComponent<ObjectData>().type == ObjectType.Wall && go.GetComponent<Collider2D>().enabled)
                    return true;
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
