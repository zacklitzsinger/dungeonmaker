using UnityEngine;

public class ShadowCloak : MonoBehaviour, IItem {

    public Sprite icon;
    public Sprite Icon {  get { return icon; } }

    public bool toggle = false;

    public float CheckCooldown()
    {
        return 0;
    }

    public void Activate(Player player)
    {
        player.actions.Add(new PlayerAction() { type = PlayerState.Shadow });
    }
}
