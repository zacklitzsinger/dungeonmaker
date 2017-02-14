using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{

    public float defaultLerp;
    public Vector2? target;

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            float allowedDistanceFromBorder = Screen.height / 2.5f;
            Vector2 topLeft = Camera.main.ScreenToWorldPoint(Vector2.one * allowedDistanceFromBorder);
            Vector2 bottomRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height) - Vector2.one * allowedDistanceFromBorder);
            Rect r = new Rect(topLeft, bottomRight - topLeft);
            MoveTowardsTarget((Vector2)target, defaultLerp);
        }
        else if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 motion = Input.GetAxisRaw("Horizontal") * Vector2.right + Input.GetAxisRaw("Vertical") * Vector2.up;
            motion /= 2;
            transform.position += motion;
        }
    }

    void MoveTowardsTarget(Vector2 pos, float lerp)
    {
        Vector3 newPosition = Vector2.Lerp(Camera.main.transform.position, pos, lerp);
        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }
}
