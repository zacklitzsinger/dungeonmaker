using UnityEngine;

public class Idle : MonoBehaviour {

    public int frames;
    private int remFrames;

    void OnEnable()
    {
        remFrames = frames;
    }

    void FixedUpdate()
    {
        if (remFrames-- <= 0)
            enabled = false;
    }
}
