using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Tab Navigator for UI
// Single instance of this script per GUI
// An alternative would be to use a next/previous setting on a single GUI item, which would mean one script per InputField - not ideal

public class UITabNavigation : MonoBehaviour
{
    private EventSystem eventSystem;

    private void Start()
    {
        eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab))
            return;

        if (eventSystem.currentSelectedGameObject == null)
        {
            // Not sure why this works but it does
            eventSystem.SetSelectedGameObject(Selectable.allSelectables[Selectable.allSelectables.Count-1].gameObject);
            return;
        }

        Selectable current = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
        if (current == null)
            return;

        bool up = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Selectable next = up ? current.FindSelectableOnUp() : current.FindSelectableOnDown();

        // We are at the end or the beginning, go to either, depends on the direction we are tabbing in
        // The previous version would take the logical 0 selector, which would be the highest up in your editor hierarchy
        // But not certainly the first item on your GUI, or last for that matter
        // This code tabs in the correct visual order
        if (next == null)
        {
            next = current;

            Selectable pnext;
            if (up) while ((pnext = next.FindSelectableOnDown()) != null) next = pnext;
            else while ((pnext = next.FindSelectableOnUp()) != null) next = pnext;
        }

        // Simulate Inputfield MouseClick
        InputField inputfield = next.GetComponent<InputField>();
        if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(eventSystem));

        // Select the next item in the taborder of our direction
        eventSystem.SetSelectedGameObject(next.gameObject);
    }
}
