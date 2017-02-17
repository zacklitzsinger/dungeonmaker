using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{

    public Color toggleColor;
    Image image;
    Toggle toggle;
    Color baseColor;

    // Use this for initialization
    void Start()
    {
        image = GetComponent<Image>();
        baseColor = image.color;
        toggle = GetComponentInParent<Toggle>();
        image.color = toggle.isOn ? toggleColor : baseColor;
        toggle.onValueChanged.AddListener((bool on) =>
        {
            image.color = on ? toggleColor : baseColor;
        });
    }
}
