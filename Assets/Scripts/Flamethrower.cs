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
        // If fire doesn't start at zero, it will be the full distance for a single frame, which tends to mess up puzzles.
        fire.size = 0;
    }

    void Update()
    {
        if (animator)
            animator.SetBool("active", active);
    }

    void FixedUpdate()
    {
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            active = circuit.Powered ^ invert;
        fire.active = active;
        float currentDistance = FindDistance();
        if (currentDistance <= fire.size)
            fire.size = currentDistance;
        else
            fire.size = Mathf.Min(fire.size + 0.016f, currentDistance);
    }

    float FindDistance()
    {
        if (!active)
            return 0;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.up, distance);
        foreach (RaycastHit2D hit in hits)
            if (!hit.collider.isTrigger && CheckForCollisions(hit.collider.gameObject))
                return Mathf.Max(0, (hit.collider.transform.position - transform.position).magnitude - 1);
        return distance;
    }

    bool CheckForCollisions(GameObject go)
    {
        ObjectData info = go.GetComponentInParent<ObjectData>();
        if (!info || info.gameObject == gameObject)
            return false;
        if (info.type == ObjectType.Wall || go.GetComponent<Health>() && go.GetComponent<Health>().invulnerableOverride)
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
