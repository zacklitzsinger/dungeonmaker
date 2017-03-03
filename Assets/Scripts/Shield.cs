using UnityEngine;

public class Shield : MonoBehaviour {

    [Tooltip("How many frames the player must idle after blocking successfully")]
    public int blockStunnedFrames;

    Player player;

    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    public void Block()
    {
        player.currentAction = new PlayerAction() { type = PlayerState.Idle, frames = blockStunnedFrames };
        player.TriggerAction(player.currentAction);
    }

}
