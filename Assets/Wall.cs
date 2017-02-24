using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

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

    SpriteRenderer spriteRenderer;
    ObjectData data;
    Circuit circuit;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        data = GetComponent<ObjectData>();
    }

    void FixedUpdate()
    {
        if (circuit == null)
            circuit = GetComponent<Circuit>();
        if (circuit)
        {
            // Default behavior of a wall should be to be a wall.
            Active = !circuit.Powered;
            gameObject.layer = Active ? LayerMask.NameToLayer("Default") : LayerMask.NameToLayer("CollisionDisabled");
            float targetAlpha = Active ? 1 : 0;
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, 0.15f);
            if (Mathf.Abs(c.a - targetAlpha) < 0.05f)
                c.a = targetAlpha;
            spriteRenderer.color = c;
            data.seeThrough = !Active;
        }
    }

}
