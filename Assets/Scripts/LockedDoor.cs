using UnityEngine;

public class LockedDoor : MonoBehaviour {

    public bool locked = true;
    public Material unlockMaterial;
    Animator animator;
    Door door;

    void Start()
    {
        animator = GetComponent<Animator>();
        door = GetComponent<Door>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (locked && collider.CompareTag("Player"))
        {
            var player = collider.GetComponentInParent<Player>();
            if (player.keys > 0)
            {
                player.keys--;
                locked = false;
                door.enabled = true;
                door.Open = true;
                GetComponent<Renderer>().material = unlockMaterial;
            }
        }
    }

    void Update()
    {
        animator.SetBool("locked", locked);
    }
}
