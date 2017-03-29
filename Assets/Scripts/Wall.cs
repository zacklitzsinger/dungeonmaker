using UnityEngine;

public class Wall : MonoBehaviour
{

    public bool active = true;
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            if (active != value)
                LevelEditor.main.currentRoomDirty = true;
            active = value;
        }
    }

    MeshRenderer meshRenderer;
    ObjectData data;
    Circuit circuit;
    IDamageable health;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        data = GetComponent<ObjectData>();
        health = GetComponent<IDamageable>();
    }

    void OnDisable()
    {
        if (!gameObject.activeSelf)
        {
            if (data)
                data.seeThrough = true;
            LevelEditor.main.currentRoomDirty = true;
        }
    }

    void FixedUpdate()
    {
        if (circuit == null)
            circuit = GetComponent<Circuit>();
        if (circuit)
        {
            // Default behavior of a wall should be to be a wall.
            Active = !circuit.Powered;
            gameObject.layer = Active ? LayerMask.NameToLayer("Wall") : LayerMask.NameToLayer("CollisionDisabled");
            data.seeThrough = !Active;

            if (LevelEditor.main.currentRoom.Contains(transform.position))
            {
                float targetAlpha = Active ? 1 : 0;
                Color c = meshRenderer.material.color;
                c.a = Mathf.Lerp(c.a, targetAlpha, 0.15f);
                if (Mathf.Abs(c.a - targetAlpha) < 0.05f)
                    c.a = targetAlpha;
                meshRenderer.material.color = c;
            }
        }
        else if (health == null)
        {
            enabled = false;
        }
    }

}
