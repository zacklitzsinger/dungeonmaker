using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour {

    public bool editing = true;
    public GameObject selectedPrefab;
    public GameObject[] prefabOptions;
    public GameObject prefabButton;
    public GameObject buttonParent;
    public Texture selectionBox;
    public int gridX = 32, gridY = 32;
    Dictionary<Vector2, GameObject> tilemap = new Dictionary<Vector2, GameObject>();

    void Start()
    {
        foreach (GameObject option in prefabOptions) {
            GameObject button = Instantiate(prefabButton, buttonParent.transform);
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            var textComponent = button.GetComponentInChildren<Text>();
            textComponent.text = option.gameObject.name;
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedPrefab = option;
            });
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Edit"))
        {
            editing = !editing;
        }
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        if (Input.GetMouseButton(0) && editing)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(ConvertPositionToGrid(Input.mousePosition));
            pos.z = 0;
            if (tilemap.ContainsKey(pos))
                return;
            GameObject newObj = Instantiate(selectedPrefab, pos, Quaternion.identity, transform);
            tilemap[pos] = newObj;
        }

    }

    void OnGUI()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !editing)
            return;
        Vector3 pos = ConvertPositionToGrid(Input.mousePosition);
        pos.y = Screen.height - pos.y;
        pos.x -= gridX / 2;
        pos.y -= gridY / 2;
        GUI.DrawTexture(new Rect(pos, new Vector2(gridX, gridY)), selectionBox);
    }

    Vector3 ConvertPositionToGrid(Vector3 pos)
    {
        pos.x = pos.x + gridX / 2 - pos.x % gridX;
        pos.y = pos.y + gridY / 2 - pos.y % gridY;
        return pos;
    }
}
