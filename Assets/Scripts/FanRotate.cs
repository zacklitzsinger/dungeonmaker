using UnityEngine;

public class FanRotate : MonoBehaviour {

    public float speed;

    Rigidbody rb;
    Fan fan;

    void Start()
    {
        fan = GetComponentInParent<Fan>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (fan.active)
            rb.AddTorque(speed * transform.up);
    }

}
