using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {

    public Sprite ItemSprite
    {
        get { return icon.sprite; }
        set {
            icon.sprite = value;
            if (value != null)
                icon.enabled = true;
        }
    }
    public Text slot;
    public Image icon;
}
