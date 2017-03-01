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
        float distance = FindDistance();
        if (distance <= wind.size)
            wind.size = distance;
        else
            wind.size = Mathf.Min(wind.size + 0.016f * activationSpeed, distance);
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
        if (info.type == ObjectType.Wall)
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
