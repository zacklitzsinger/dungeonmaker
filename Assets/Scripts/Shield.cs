using UnityEngine;

public class Shield : MonoBehaviour {

    [Tooltip("How many frames the player must idle after blocking successfully")]
    public int blockStunnedFrames;

    public AudioClip blockSound;

    IActionQueue actionQueue;

    void Start()
    {
        actionQueue = GetComponentInParent<IActionQueue>();
    }

    public void Block(IActionQueue source)
    {
        if (source != null)
            source.Interrupt(blockStunnedFrames);
        actionQueue.Interrupt(blockStunnedFrames);
        AudioSource.PlayClipAtPoint(blockSound, transform.position);
    }

}
