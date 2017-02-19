using UnityEngine;

public class LockedDoor : MonoBehaviour {

    public bool locked = true;
    Animator animator;
    Door door;

    void Start()
    {
        animator = GetComponent<Animator>();
        door = GetComponent<Door>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (locked && collider.tag == "Player")
        {
            var player = collider.GetComponentInParent<Player>();
            if (player.keys > 0)
            {
                player.keys--;
                locked = false;
            }
        }
    }

    void Update()
    {
        door.enabled = !locked;
        animator.SetBool("locked", locked);
    }
}
