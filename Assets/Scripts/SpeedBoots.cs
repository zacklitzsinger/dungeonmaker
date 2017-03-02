using UnityEngine;

public class SpeedBoots : MonoBehaviour, IItem
{
    public int chargeFrames;

    public Sprite icon;
    public Sprite Icon { get { return icon; } }

    public float CheckCooldown()
    {
        return 0;
    }

    public void Activate(Player player)
    {
        Vector2 targetDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position).normalized;
        player.actions.Enqueue(new PlayerAction() { type = PlayerState.ChargingDash, frames = chargeFrames});
        player.actions.Enqueue(new PlayerAction() { type = PlayerState.Dash, vector = targetDirection });
    }
}

