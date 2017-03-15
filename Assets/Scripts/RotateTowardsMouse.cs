using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour {

    void FixedUpdate()
    {
        Vector2 targetDirection = (LevelEditor.main.GetXYPlanePosition(Input.mousePosition) - (Vector2)transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, targetDirection);
    }
}
