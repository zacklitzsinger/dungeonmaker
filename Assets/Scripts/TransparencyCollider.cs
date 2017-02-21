using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparencyCollider : MonoBehaviour {

    HashSet<SpriteRenderer> fadingOut = new HashSet<SpriteRenderer>();
    HashSet<SpriteRenderer> fadingIn = new HashSet<SpriteRenderer>();

    void Start()
    {
        LevelEditor.main.onRoomChanged.AddListener(() =>
        {
            fadingOut.Clear();
            fadingIn.Clear();
        });
    }

    void OnTriggerStay2D(Collider2D other)
    {
        SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
        ObjectData data = other.GetComponent<ObjectData>();
        if (sr == null || data == null || data.type != ObjectType.Wall || !LevelEditor.main.currentRoom.Contains(new MapNode(other.transform.position)))
            return;
        fadingOut.Add(sr);
        fadingIn.Remove(sr);
    }

    void FixedUpdate()
    {
        foreach (SpriteRenderer renderer in fadingOut)
        {
            Color c = renderer.color;
            c.a = Mathf.Max(0.5f, c.a - 0.032f);
            renderer.color = c;
        }

        foreach (SpriteRenderer renderer in fadingIn)
        {
            Color c = renderer.color;
            c.a = Mathf.Min(1f, c.a + 0.032f);
            renderer.color = c;
        }
        fadingIn.RemoveWhere((sr) => { return sr.color.a >= 1f; });
    }

    void OnTriggerExit2D(Collider2D other)
    {
        SpriteRenderer sr = other.GetComponent<SpriteRenderer>();
        ObjectData data = other.GetComponent<ObjectData>();
        if (sr == null || data == null || data.type != ObjectType.Wall || !LevelEditor.main.currentRoom.Contains(new MapNode(other.transform.position)))
            return;
        fadingOut.Remove(sr);
        fadingIn.Add(sr);
    }
}
