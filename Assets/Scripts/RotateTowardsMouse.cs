using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour {

    void FixedUpdate()
    {
        Vector2 targetDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
    }
}
