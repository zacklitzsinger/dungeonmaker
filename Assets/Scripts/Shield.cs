using UnityEngine;

public class Shield : MonoBehaviour
{

    [Tooltip("How many frames the player must idle after blocking successfully")]
    public int blockStunnedFrames;

    public float maxBlockAngle;
    public AudioClip blockSound;

    IActionQueue actionQueue;

    void Start()
    {
        actionQueue = GetComponentInParent<IActionQueue>();
    }

    public bool TryBlock(Vector2 direction, IActionQueue source)
    {
        float angle = Vector3.Angle(direction, transform.up);
        if (angle > maxBlockAngle)
            return false;
        Block(source);
        return true;
    }

    public void Block(IActionQueue source)
    {
        if (source != null)
            source.Interrupt(blockStunnedFrames);
        if (actionQueue != null)
            actionQueue.Interrupt(blockStunnedFrames);
        AudioSource.PlayClipAtPoint(blockSound, transform.position);
    }

}
