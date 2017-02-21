using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class EventSystemExtensions {

    /// <summary>
    /// Checks if an input field is currently focused.
    /// </summary>
    /// <returns>Returns true if a field is focused.</returns>
    public static bool IsFieldFocused(this EventSystem es)
    {
        GameObject selected = es.currentSelectedGameObject;
        if (selected == null)
            return false;
        InputField field = selected.GetComponent<InputField>();
        if (field == null)
            return false;
        return field.isFocused;
    }

}
