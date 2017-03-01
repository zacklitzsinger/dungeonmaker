using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour {

    public int delay;
    int selectedFrameCount;

    Image image;
    Text text;
    Tooltip selectedTooltip;

	// Use this for initialization
	void Start () {
        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
	}
	
    void SetText(string t = "")
    {
        image.enabled = (t.Length > 0);
        text.enabled = (t.Length > 0);
        text.text = t;
    }

    Tooltip GetTooltipUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        foreach (RaycastResult result in results)
        {
            Tooltip t = result.gameObject.GetComponentInParent<Tooltip>();
            if (!t)
                continue;
            return t;
        }
        return null;
    }

	// Update is called once per frame
	void Update () {
        
        SetText();
        Tooltip current = GetTooltipUnderMouse();
        if (current != selectedTooltip)
        {
            selectedTooltip = current;
            selectedFrameCount = delay;
        }

        if (selectedFrameCount > 0)
            selectedFrameCount--;
        else if (selectedTooltip)
            SetText(selectedTooltip.text);

        transform.position = Input.mousePosition;
    }
}
