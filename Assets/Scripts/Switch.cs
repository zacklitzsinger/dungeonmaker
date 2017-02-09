using UnityEngine;

public class Switch : MonoBehaviour {

    public bool active = false;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        active = true;
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            circuit.AdjustPower(1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        active = false;
        Circuit circuit = GetComponent<Circuit>();
        if (circuit)
            circuit.AdjustPower(-1);
    }

    void FixedUpdate()
    {
        animator.SetBool("active", active);
    }
}
