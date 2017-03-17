using UnityEngine;

public class VisionCone : MonoBehaviour {

    public float angle;
    public float distance;
    public Transform target;
    public LayerMask mask;

    public MonoBehaviour state;

    public bool alwaysTrackPlayer;

    Transform CheckAngle(float checkAngle)
    {
        Vector2 direction = (Quaternion.AngleAxis(checkAngle, Vector3.forward) * (transform.up)).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, mask | LayerMask.NameToLayer("Player"));
        Debug.DrawLine(transform.position, direction * distance + (Vector2)transform.position, Color.red);
        if (hit && hit.transform.CompareTag("Player"))
            return hit.transform;
        return null;
    }

    Transform CheckFOV()
    {
        if (angle <= 0)
            return null;
        Transform t = null;
        // Not optimal, but better for debugging purposes
        for (float a = -angle/2; a <= angle/2; a += angle/5)
        {
            t = CheckAngle(a) ?? t;
        }
        return t;
    }

    void FixedUpdate()
    {
        target = CheckFOV();
    }

}
