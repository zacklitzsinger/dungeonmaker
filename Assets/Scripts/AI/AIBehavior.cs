using UnityEngine;

public abstract class AIBehavior : MonoBehaviour
{
    public virtual bool CanRelinquishControl()
    {
        return true;
    }
}

