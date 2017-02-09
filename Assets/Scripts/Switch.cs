using UnityEngine;

public class Switch : MonoBehaviour {

    public bool active = false;

    Circuit circuit;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        circuit = GetComponent<Circuit>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        active = true;
        circuit.AdjustPower(1);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        active = false;
        circuit.AdjustPower(-1);
    }

    void FixedUpdate()
    {
        animator.SetBool("active", active);
    }
}
