using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum EditMode
{
    None,
    Play,
    Victory,
    // Modes below here cause timescale to be set to zero
    Create,
    Edit,
    Circuit
}

[Serializable]
public class PrefabGroup
{
    public string name;
    public GameObject[] prefabs;
}

public class LevelEditor : MonoBehaviour, ICustomSerializable
{
    public static LevelEditor main;

    public EditMode mode = EditMode.Create;
    /// <summary>
    /// Can we edit the level and change modes to level editing modes?
    /// </summary>
    public bool canEdit = false;
    [ReadOnly]
    public GameObject selectedPrefab;
    [ReadOnly]
    public GameObject selectedGameObject;
    public PrefabGroup[] prefabGroups;
    private GameObject[] allPrefabOptions;
    [ReadOnly]
    public float rotation;
    [ReadOnly]
    /// <summary>
    /// Number of frames since play started.
    /// </summary>
    public int playFrames;

    // UI
    public GameObject sidebar;
    public Text editModeLabel;
    public GameObject prefabButton;
    public GameObject sidebarContent;
    public Texture selectionBox;
    public Toggle prefabToggle;
    public GameObject prefabIntSlider;
    public GameObject prefabDropdown;
    public ToggleGroup tabPanel;
    public Toggle prefabToggleButton;
    public GameObject victoryPanel;
    public Text victoryTimeText;
    public GameObject savePanel;
    public InputField saveNameInput;
    public GameObject uploadPanel;
    public InputField uploadNameInput;
    public InputField uploadDescInput;
    public GameObject uploadProgress;
    public GameObject playerPanel;
    public GameObject helpPanel;

    // Circuits
    public Color circuitColor;
    public Color selectedCircuitInputColor;
    public Color selectedCircuitOutputColor;
    public Color selectedConnectionColor;

    // Level information
    public string levelName;
    public string levelDesc;
    public Dictionary<Vector2, List<ObjectData>> tilemap = new Dictionary<Vector2, List<ObjectData>>();
    public Dictionary<Guid, GameObject> guidmap = new Dictionary<Guid, GameObject>();
    public NavMap navmap;
    public NavigationCalculator navcalc;
    [ReadOnly]
    public List<Vector2> previousRoom;
    [ReadOnly]
    public List<Vector2> currentRoom = new List<Vector2>();
    public bool currentRoomDirty = false; // Set to true to guarantee the room is recalculated next frame.
    public UnityEvent onRoomChanged;

    /// <summary>
    /// When testing, save to a temporary file beforehand so we can reload the level after finishing
    /// </summary>
    private string tempFilename;
    Vector2 lastMousePosition;
    /// <summary>
    /// Was the app focused last frame? If false, app just became focused, so we should ignore many UI inputs.
    /// </summary>
    bool lastAppFocused = true;

    void Awake()
    {
        main = this;
        navmap = new NavMap(tilemap);
        navcalc = new NavigationCalculator(navmap);

        List<GameObject> prefabs = new List<GameObject>();
        foreach (PrefabGroup group in prefabGroups)
            prefabs.AddRange(group.prefabs);
        allPrefabOptions = prefabs.ToArray();
    }

    void Start()
    {

        SetHelpTextMode(mode);
        SidebarCreateButtons();

        if (saveNameInput != null)
            saveNameInput.onValueChanged.AddListener((string str) => { levelName = str; });

        if (uploadNameInput != null)
            uploadNameInput.onValueChanged.AddListener((string str) => { levelName = str; });

        if (uploadDescInput != null)
            uploadDescInput.onValueChanged.AddListener((string str) => { levelDesc = str; });

        if (victoryPanel != null)
        {
            Button button = victoryPanel.GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                if (canEdit)
                    ChangeMode(EditMode.Create);
                else
                    SceneManager.LoadScene("MainMenu");
            });
        }

        // Pause time while editing
        Time.timeScale = (mode >= EditMode.Create ? 0 : 1);
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
            lastAppFocused = false;
    }

    /// <summary>
    /// Creates the prefab selection buttons in the sidebar
    /// </summary>
    void SidebarCreateButtons()
    {
        if (sidebarContent == null)
            return;
        foreach (PrefabGroup group in prefabGroups)
        {
            Toggle toggleButton = Instantiate(prefabToggleButton, tabPanel.transform);
            toggleButton.name = group.name;
            toggleButton.isOn = (group == prefabGroups[0]);
            toggleButton.group = tabPanel;
            toggleButton.GetComponentInChildren<Text>().text = group.name;
            toggleButton.onValueChanged.AddListener((bool val) =>
            {
                if (val)
                    SidebarSelectGroup(group.name);
            });

            foreach (GameObject option in group.prefabs)
            {
                GameObject button = Instantiate(prefabButton, sidebarContent.transform);
                button.name = option.name;
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                var textComponent = button.GetComponentInChildren<Text>();
                textComponent.text = option.gameObject.name;
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    rotation = 0f;
                    selectedPrefab = option;
                });
            }
        }

        SidebarSelectGroup(prefabGroups[0].name);
    }

    void SidebarSelectGroup(string groupName)
    {
        foreach (Transform child in sidebarContent.transform)
        {
            PrefabGroup childGroup = Array.Find(prefabGroups, (group) =>
            {
                return Array.Find(group.prefabs, (i) => { return i.name == child.name; });
            });
            child.gameObject.SetActive(childGroup != null && childGroup.name == groupName);
        }
    }

    /// <summary>
    /// Removes all items from the sidebar
    /// </summary>
    void ClearSidebar()
    {
        if (tabPanel != null)
            foreach (Transform child in tabPanel.transform)
                Destroy(child.gameObject);
        if (sidebarContent != null)
            foreach (Transform child in sidebarContent.transform)
                Destroy(child.gameObject);
    }

    public void OpenSaveWindow(bool val)
    {
        savePanel.SetActive(val);
    }

    public void OpenUploadWindow(bool val)
    {
        uploadPanel.SetActive(val);
    }

    void SetHelpPanelText(string text = "")
    {
        if (!helpPanel)
            return;
        helpPanel.SetActive(text.Length > 0);
        helpPanel.GetComponentInChildren<Text>().text = text;
    }

    void SetHelpTextMode(EditMode mode)
    {
        switch (mode)
        {

            case EditMode.Create:
                SetHelpPanelText("Place new objects into the level. Left click to place the selected object. Right click to remove objects.");
                break;
            case EditMode.Edit:
                SetHelpPanelText("Edit properties of objects. Left click to select an object to edit.");
                break;
            case EditMode.Circuit:
                SetHelpPanelText("Connect elements via wires. Left click on a input and then an output to place a wire. Right click an object to remove all wires connected to it.");
                break;
            case EditMode.Victory:
                SetHelpPanelText();
                break;
            case EditMode.Play:
                SetHelpPanelText();
                break;
        }
    }

    /// <summary>
    /// Change current level edit mode from one mode to another
    /// </summary>
    public void ChangeMode(EditMode newMode)
    {
        if (mode == newMode)
            return;
        EditMode prevMode = mode;
        mode = newMode;
        selectedGameObject = null;
        ClearSidebar();
        // Pause time while editing
        Time.timeScale = (mode >= EditMode.Create ? 0 : 1);
        SetHelpTextMode(mode);
        switch (mode)
        {

            case EditMode.Create:
                SidebarCreateButtons();
                break;
            case EditMode.Victory:
                SetHelpPanelText();
                TimeSpan time = TimeSpan.FromMilliseconds(playFrames * 1000f / 60f);
                victoryTimeText.text = string.Format("{0}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
                break;
            case EditMode.Play:
                playFrames = 0;
                if (canEdit)
                    SaveToTemp();
                break;
        }
        if ((prevMode == EditMode.Play || prevMode == EditMode.Victory) && mode >= EditMode.Create)
        {
            currentRoom.Clear();
            LoadFromTemp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.main.Open)
            return;

        if (canEdit && !EventSystem.current.IsFieldFocused())
        {
            if (Input.GetButtonDown("Create Mode"))
                ChangeMode(EditMode.Create);
            else if (Input.GetButtonDown("Edit Mode"))
                ChangeMode(EditMode.Edit);
            else if (Input.GetButtonDown("Circuit Mode"))
                ChangeMode(EditMode.Circuit);
            else if (Input.GetButtonDown("Test Mode"))
                ChangeMode(EditMode.Play);
            if (Input.GetButtonDown("Rotate CW"))
                rotation = (rotation + 90f) % 360;
            else if (Input.GetButtonDown("Rotate CCW"))
                rotation = (rotation + 270f) % 360;
        }

        if (editModeLabel != null)
            editModeLabel.text = mode.ToString();

        // Hide editing sidebar while editing
        if (sidebar != null)
            sidebar.SetActive(mode >= EditMode.Create);

        if (victoryPanel != null)
            victoryPanel.SetActive(mode == EditMode.Victory);

        if (EventSystem.current.IsPointerOverGameObject() || mode == EditMode.Play)
            return;

        if (lastAppFocused)
        {
            switch (mode)
            {
                case EditMode.Create:

                    // Allow placing of objects by left clicking
                    if (Input.GetMouseButton(0) && selectedPrefab != null)
                    {
                        foreach (Vector2 point in GetGridPointsAlongLine(lastMousePosition, Input.mousePosition))
                            CreateSelectedPrefabAtGridPosition(point, rotation);
                        navmap.RecalculateBounds();
                    }

                    // Allow removal of objects by right clicking
                    if (Input.GetMouseButton(1))
                    {
                        // If the mouse hasn't moved, the player only wants to delete the top most item, so trigger only on mouse press.
                        if (GetGridMousePosition() == GetScreenGridPosition(lastMousePosition))
                        {
                            if (Input.GetMouseButtonDown(1))
                            {
                                Vector2 gridPos = GetGridMousePosition();
                                GameObject go = GetGameObjectAtPoint(gridPos);
                                if (go != null)
                                    DestroyGameObjectAtGridPosition(gridPos, go.GetComponent<ObjectData>());
                            }
                        }
                        // When dragging the mouse, destroy all objects along the path
                        // TODO: Maybe shouldn't count the initial point on the line
                        else
                            foreach (Vector2 point in GetGridPointsAlongLine(lastMousePosition, Input.mousePosition))
                                DestroyAllGameObjectsAtGridPosition(point);
                        navmap.RecalculateBounds();
                    }
                    break;

                case EditMode.Edit:

                    // Start by selecting an object
                    if (Input.GetMouseButtonDown(0))
                    {
                        selectedGameObject = GetGameObjectAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        if (selectedGameObject)
                        {
                            ClearSidebar();
                            UIEditSelectedGameObject();
                        }
                    }

                    break;

                case EditMode.Circuit:
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject go = GetGameObjectAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        if (!selectedGameObject)
                        {
                            // Start creating a connection
                            selectedGameObject = go;
                        }
                        // Finish placing a connection
                        else
                        {
                            // Picked same object twice...
                            if (go == selectedGameObject)
                                break;
                            if (go)
                            {
                                Circuit circuit = go.GetComponent<Circuit>() ?? go.AddComponent<Circuit>();
                                Circuit otherCircuit = selectedGameObject.GetComponent<Circuit>() ?? selectedGameObject.AddComponent<Circuit>();
                                if (circuit)
                                {
                                    otherCircuit.Connect(circuit);
                                    if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                                        selectedGameObject = null;
                                }
                            }
                        }
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        if (selectedGameObject)
                            selectedGameObject = null;
                        else
                        {
                            GameObject go = GetGameObjectAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                            if (go)
                            {
                                Circuit circuit = go.GetComponent<Circuit>();
                                if (circuit)
                                    circuit.Disconnect();
                            }
                        }
                    }
                    break;
            }
        }
        lastMousePosition = Input.mousePosition;
        lastAppFocused = true;
    }

    void FixedUpdate()
    {
        if (mode == EditMode.Play)
            playFrames++;
    }

    /// <summary>
    /// Gets points in the grid along a line.
    /// </summary>
    List<Vector2> GetGridPointsAlongLine(Vector2 from, Vector2 to, int pxFreq = 8)
    {
        List<Vector2> list = new List<Vector2>();
        list.Add(from);
        int count = (int)Math.Floor((to - from).magnitude / pxFreq);
        for (int i = 1; i <= count; i++)
            list.Add(from + (to - from).normalized * i * pxFreq);
        list.Add(to);
        List<Vector2> gridPoints = new List<Vector2>();
        foreach (Vector2 item in list)
        {
            Vector2 point = ConvertPositionToGrid(Camera.main.ScreenToWorldPoint(item));
            if (!gridPoints.Contains(point))
                gridPoints.Add(point);
        }
        return gridPoints;
    }

    /// <summary>
    /// Instantiates a copy of the currently selected prefab at the given grid position with the given rotation.
    /// </summary>
    GameObject CreateSelectedPrefabAtGridPosition(Vector2 gridPos, float rotation = 0f)
    {
        ObjectData data = selectedPrefab.GetComponent<ObjectData>();
        if (data == null)
            throw new Exception("Selected prefab missing ObjectData");
        if (!tilemap.ContainsKey(gridPos))
            tilemap[gridPos] = new List<ObjectData>();
        ObjectData sameGroup = tilemap[gridPos].Find((info) => { return info.type == data.type; });
        if (sameGroup)
        {
            // Don't replace if it's the same exact type of object
            if (sameGroup.name == selectedPrefab.name)
                return null;
            DestroyGameObjectAtGridPosition(gridPos, sameGroup);
        }
        GameObject go = CreateObjectAtGrid(gridPos, selectedPrefab);
        Guid id = Guid.NewGuid();
        go.GetComponent<ObjectData>().guid = id;
        guidmap[id] = go;
        go.transform.Rotate(Vector3.back * rotation);
        return go;
    }

    /// <summary>
    /// Destroy all game objects at the grid position in the tilemap.
    /// </summary>
    /// <param name="gridPos"></param>
    void DestroyAllGameObjectsAtGridPosition(Vector2 gridPos)
    {
        if (!tilemap.ContainsKey(gridPos))
            return;
        List<ObjectData> goList = tilemap[gridPos];
        while (goList.Count > 0)
            DestroyGameObjectAtGridPosition(gridPos, goList[0]);
    }

    /// <summary>
    /// Destroy a single game object at the grid position in the tilemap.
    /// </summary>
    void DestroyGameObjectAtGridPosition(Vector2 gridPos, ObjectData data)
    {
        if (!tilemap.ContainsKey(gridPos))
            return;
        List<ObjectData> goList = tilemap[gridPos];
        goList.Remove(data);
        if (goList.Count == 0)
            tilemap.Remove(gridPos);
        Guid id = data.guid;
        guidmap.Remove(id);
        Destroy(data.gameObject);
    }

    /// <summary>
    /// Returns the grid position under the mouse.
    /// </summary>
    /// <returns></returns>
    Vector2 GetGridMousePosition()
    {
        return GetScreenGridPosition(Input.mousePosition);
    }

    /// <summary>
    /// Returns the grid position under the given screen coordinates.
    /// </summary>
    Vector2 GetScreenGridPosition(Vector2 pos)
    {
        return ConvertPositionToGrid(Camera.main.ScreenToWorldPoint(pos));
    }

    /// <summary>
    /// Instantiates given prefab at the point on the grid
    /// </summary>
    public GameObject CreateObjectAtGrid(Vector2 point, GameObject obj)
    {
        GameObject newObj = Instantiate(obj, point, Quaternion.identity, transform);
        newObj.name = obj.name;
        if (!tilemap.ContainsKey(point))
            tilemap[point] = new List<ObjectData>();
        tilemap[point].Add(newObj.GetComponent<ObjectData>());
        return newObj;
    }

    /// <summary>
    /// Get the topmost tile at the grid point.
    /// </summary>
    GameObject GetGameObjectAtPoint(Vector2 point)
    {
        point = ConvertPositionToGrid(point);
        if (!tilemap.ContainsKey(point))
            return null;
        ObjectData choice = null;
        foreach (ObjectData data in tilemap[point])
        {
            // Must be an object the level editor knows about
            if (data.transform.parent != transform)
                continue;
            if (choice == null)
                choice = data;
            else if (choice.type < data.type)
                choice = data;
        }
        return choice ? choice.gameObject : null;
    }

    /// <summary>
    /// Get an object at the position in the tilemap by type.
    /// </summary>
    public GameObject GetGameObjectAtPointWithType(Vector2 point, ObjectType type)
    {
        point = ConvertPositionToGrid(point);
        if (!tilemap.ContainsKey(point))
            return null;
        ObjectData info = tilemap[point].Find((data) =>
        {
            return (data.transform.parent == transform && data.type == type);
        });
        return info != null ? info.gameObject : null;
    }

    /// <summary>
    /// Set the currently active room. Uses NavMap to determine what is contained in the current room.
    /// </summary>
    public bool SetCurrentRoom(Vector2 gridPos)
    {
        Vector2 currentNode = ConvertPositionToGrid(gridPos);
        if (currentRoom.Contains(currentNode) && !currentRoomDirty)
            return false;
        previousRoom = currentRoom;
        currentRoom = navcalc.GetConnectedNodes(currentNode, true, true);
        // Iterate only through the original list.
        int nodeCount = currentRoom.Count;
        for (int i = 0; i < nodeCount; i++)
            foreach (Vector2 node in navmap.GetPotentialNeighbors(currentRoom[i]))
                if (!currentRoom.Contains(node))
                    currentRoom.Add(node);

        // Update visibility
        foreach (KeyValuePair<Vector2, List<ObjectData>> pair in tilemap)
        {
            bool active = currentRoom.Contains(pair.Key);
            foreach (ObjectData data in pair.Value)
                if (data != null)
                    SetTileVisibility(data.gameObject, data.CompareTag("Player") || active);
        }

        // Update camera
        Vector2 avg = Vector2.zero;
        foreach (Vector2 node in currentRoom)
            avg += new Vector2(node.x, node.y);
        avg /= currentRoom.Count;
        onRoomChanged.Invoke();
        Camera.main.GetComponent<CameraFollow>().SetTarget(avg);
        currentRoomDirty = false;
        return true;
    }

    /// <summary>
    /// Shows or hides a given tile (based on whether it is in the current room).
    /// </summary>
    void SetTileVisibility(GameObject go, bool active, bool immediate = false)
    {
        if (go == null)
            return;
        foreach (SpriteRenderer renderer in go.GetComponentsInChildren<SpriteRenderer>())
        {
            float targetAlpha = active ? 1f : 0f;
            if (immediate)
            {
                Color c = renderer.color;
                c.a = targetAlpha;
                renderer.color = c;
            }
            // This is a hack - we want to skip doors and walls that have become see through.
            else if (go.layer != LayerMask.NameToLayer("CollisionDisabled") || go.GetComponent<ObjectData>().type != ObjectType.Wall)
            {
                StartCoroutine(ControlAlpha(renderer, targetAlpha));
            }
        }
        // Enable/disable particle systems (including inactive ones).
        foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>(true))
            ps.gameObject.SetActive(active);
    }

    public IEnumerator ControlAlpha(SpriteRenderer r, float targetAlpha)
    {
        float alpha = r.color.a;
        while (alpha != targetAlpha && r != null)
        {
            alpha = Mathf.Lerp(alpha, targetAlpha, 0.15f);
            if (Mathf.Abs(alpha - targetAlpha) < 0.05f)
                alpha = targetAlpha;
            Color c = r.color;
            c.a = alpha;
            r.color = c;
            yield return new WaitForFixedUpdate();
        }
    }

    public void HitPause(int duration)
    {
        StartCoroutine(PauseForDuration(duration));
    }

    IEnumerator PauseForDuration(int duration)
    {
        Time.timeScale = 0f;
        while (duration-- >= 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 1;
    }

    /// <summary>
    /// Creates the UI for editing a game object. Monobehaviors that implement ICustomSerializable 
    /// and have fields that are marked [PlayerEditable] will be given UI elements for the player to edit.
    /// </summary>
    void UIEditSelectedGameObject()
    {
        // Create UI for editable game object
        foreach (ICustomSerializable component in selectedGameObject.GetComponents<ICustomSerializable>())
        {
            foreach (FieldInfo field in component.GetType().GetFields())
            {
                foreach (PlayerEditableAttribute attr in field.GetCustomAttributes(typeof(PlayerEditableAttribute), true))
                {
                    if (field.FieldType == typeof(bool))
                    {
                        Toggle toggle = Instantiate(prefabToggle, sidebarContent.transform);
                        toggle.isOn = (bool)field.GetValue(component);
                        toggle.GetComponentInChildren<Text>().text = attr.Name;
                        toggle.onValueChanged.AddListener((val) =>
                        {
                            field.SetValue(component, (bool)val);
                        });
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        var rangeAttr = attr as PlayerEditableRangeAttribute;
                        GameObject range = Instantiate(prefabIntSlider, sidebarContent.transform);
                        Slider slider = range.GetComponentInChildren<Slider>();
                        slider.wholeNumbers = true;
                        slider.minValue = rangeAttr.Min;
                        slider.maxValue = rangeAttr.Max;
                        slider.value = (int)field.GetValue(component);
                        Text text = range.GetComponentInChildren<Text>();
                        text.text = rangeAttr.Name + ": " + slider.value;
                        slider.onValueChanged.AddListener((val) =>
                        {
                            field.SetValue(component, (int)val);
                            text.text = rangeAttr.Name + ": " + val;
                        });
                    }
                    else if (field.FieldType.IsEnum)
                    {
                        var enumAttr = attr as PlayerEditableEnumAttribute;
                        GameObject labeledDropdown = Instantiate(prefabDropdown, sidebarContent.transform);
                        Text label = labeledDropdown.GetComponentInChildren<Text>();
                        label.text = enumAttr.Name + ":";
                        Dropdown dropdown = labeledDropdown.GetComponentInChildren<Dropdown>();
                        dropdown.AddOptions(new List<string>(enumAttr.Choices));
                        dropdown.value = (int)field.GetValue(component);
                        dropdown.onValueChanged.AddListener((val) =>
                        {
                            field.SetValue(component, (int)val);
                        });
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        switch (mode)
        {
            case EditMode.Create:
                if (EventSystem.current.IsPointerOverGameObject() || PauseMenu.main.Open)
                    return;
                // Draw currently selected grid square
                Vector2 gridPos = ConvertPositionToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                gridPos += Vector2.up;
                Vector2 screenPos = Camera.main.WorldToScreenPoint(gridPos);
                screenPos.y = Screen.height - screenPos.y;
                screenPos -= Vector2.one * Constants.GRID_SIZE / 2;
                GUI.DrawTexture(new Rect(screenPos, new Vector2(selectionBox.width, selectionBox.height)), selectionBox);
                if (selectedPrefab)
                {
                    Sprite sprite = selectedPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
                    Vector2 pos = Input.mousePosition;
                    pos.y = Screen.height - pos.y;
                    // Tex coords are in % of the full texture rather than being a direct source rectangle
                    Rect texCoords = new Rect(sprite.rect.x / sprite.texture.width, sprite.rect.y / sprite.texture.height, sprite.rect.width / sprite.texture.width, sprite.rect.height / sprite.texture.height);
                    // Rotate texture
                    Matrix4x4 matrixTemp = GUI.matrix;
                    GUIUtility.RotateAroundPivot(rotation, pos);
                    GUI.DrawTextureWithTexCoords(new Rect(pos - sprite.rect.size / 2, sprite.rect.size), sprite.texture, texCoords);
                    GUI.matrix = matrixTemp;
                }

                break;

            case EditMode.Edit:
                if (selectedGameObject)
                {
                    Vector2 rectPoint = Camera.main.WorldToScreenPoint((Vector2)selectedGameObject.transform.position + Vector2.up);
                    rectPoint.y = Screen.height - rectPoint.y;
                    GUI.DrawTexture(new Rect(rectPoint - Vector2.one * Constants.GRID_SIZE / 2, new Vector2(selectionBox.width, selectionBox.height)), selectionBox);
                }

                break;

            case EditMode.Circuit:
                Line line = Camera.main.GetComponent<Line>();
                // Draw line from selected object to mouse if we are placing a circuit
                if (selectedGameObject && !PauseMenu.main.Open)
                {
                    line.DrawArrow(selectedGameObject.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), selectedConnectionColor);
                }

                // Draw circuits - technically draws each line twice, but shouldn't matter
                foreach (Circuit circuit in GetComponentsInChildren<Circuit>())
                {
                    Color c = circuitColor;
                    if (circuit.gameObject == selectedGameObject)
                        c = selectedCircuitOutputColor;
                    foreach (Circuit output in circuit.outputs)
                    {
                        if (output.gameObject == selectedGameObject)
                            c = selectedCircuitInputColor;
                        line.DrawArrow(circuit.transform.position, output.transform.position, c);
                    }
                }
                break;
        }
    }

    public void SaveToDisk()
    {
        if (levelName.Length == 0)
            return;
        string filename = Path.Combine(Application.persistentDataPath, levelName);
        SaveToStream(File.Create(filename));
    }

    public string SaveToTemp()
    {
        tempFilename = Path.GetTempFileName();
        SaveToStream(File.Create(tempFilename));
        return tempFilename;
    }

    public void SaveToStream(Stream s)
    {
        using (BinaryWriter bw = new BinaryWriter(s))
            Serialize(bw);
    }

    public void UploadLevel()
    {
        SaveToTemp();
        StartCoroutine(UploadFile(tempFilename));
    }

    public IEnumerator UploadFile(string filename)
    {
        WWWForm form = new WWWForm();
        form.AddField("levelName", levelName);
        form.AddField("description", levelDesc);
        form.AddBinaryData("level", File.ReadAllBytes(filename));
        string url = WebServer.SERVER + "/levels";
        WWW www = new WWW(url, form);
        if (uploadProgress != null)
            uploadProgress.SetActive(true);
        while (www.error != null && www.uploadProgress < 1)
        {
            uploadProgress.GetComponentInChildren<ProgressBar>().percentage = www.uploadProgress;
            yield return new WaitForFixedUpdate();
        }
        if (www.error != null)
            Debug.Log(www.error);
        else
            Debug.Log("Finished uploading level to: " + url);
        if (uploadProgress != null)
            uploadProgress.SetActive(false);
    }

    public void LoadFromDisk(string filename)
    {
        LoadFromStream(File.OpenRead(filename));
    }

    public void LoadFromStream(Stream s)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        tilemap.Clear();
        guidmap.Clear();
        using (BinaryReader br = new BinaryReader(s))
            Deserialize(br);
    }

    public void LoadFromTemp()
    {
        LoadFromStream(File.OpenRead(tempFilename));
        File.Delete(tempFilename);
    }

    public void LoadFromBytes(byte[] bytes)
    {
        using (Stream stream = new MemoryStream(bytes))
            LoadFromStream(stream);
    }

    /// <summary>
    /// Rounds the given position to the grid. Assumes world position.
    /// </summary>
    public Vector2 ConvertPositionToGrid(Vector2 pos)
    {
        return pos.ToGrid();
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(levelName);
        bw.Write(tilemap.Count);
        foreach (KeyValuePair<Vector2, List<ObjectData>> pair in tilemap)
        {
            bw.Write(pair.Key.x);
            bw.Write(pair.Key.y);
            bw.Write(pair.Value.Count);
            foreach (ObjectData data in pair.Value)
            {
                bw.Write(data.name);
                Guid id = data.guid;
                bw.Write(id);
            }
        }
        bw.Write(guidmap.Count);
        foreach (KeyValuePair<Guid, GameObject> pair in guidmap)
        {
            bw.Write(pair.Key);
            SerializeComponents(pair.Value, bw);
        }
    }

    /// <summary>
    /// Serialize the components on a given game object
    /// </summary>
    void SerializeComponents(GameObject go, BinaryWriter bw)
    {
        var components = go.GetComponents<ICustomSerializable>();
        bw.Write(components.Length);
        foreach (ICustomSerializable s in components)
        {
            bw.Write(s.GetType().ToString());
            s.Serialize(bw);
        }

        // TODO: Write something to signify that we are done?
    }

    public void Deserialize(BinaryReader br)
    {
        levelName = br.ReadString();
        if (saveNameInput != null)
            saveNameInput.text = levelName;
        if (uploadNameInput != null)
            uploadNameInput.text = levelName;
        int tileCount = br.ReadInt32();
        for (int i = 0; i < tileCount; i++)
        {
            Vector2 pos = new Vector2(br.ReadSingle(), br.ReadSingle());
            int goCount = br.ReadInt32();
            List<GameObject> goList = new List<GameObject>();
            for (int j = 0; j < goCount; j++)
            {
                // TODO: Should separate deserialization with instantiating game objects so levels can easily be reset
                string goName = br.ReadString();
                Guid id = br.ReadGuid();
                GameObject prefab = Array.Find(allPrefabOptions, (o) => { return o.name == goName; });
                if (prefab == null)
                {
                    throw new Exception("Could not find prefab in level named " + goName);
                }
                GameObject go = CreateObjectAtGrid(pos, prefab);
                go.GetComponent<ObjectData>().guid = id;
                goList.Add(go);
                guidmap[id] = go;
            }
        }
        int guidCount = br.ReadInt32();
        for (int i = 0; i < guidCount; i++)
        {
            Guid id = br.ReadGuid();
            GameObject go = guidmap[id];
            // In play mode, start by hiding all tiles
            if (mode == EditMode.Play)
                SetTileVisibility(go, false, true);
            DeserializeComponents(go, br);
        }
        navmap.RecalculateBounds();
    }

    /// <summary>
    /// Deserialize the components on a given game object.
    /// </summary>
    void DeserializeComponents(GameObject go, BinaryReader br)
    {
        var count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            String typeName = br.ReadString();
            var s = (ICustomSerializable)(go.GetComponent(typeName) ?? go.AddComponent(Type.GetType(typeName)));
            s.Deserialize(br);
        }
    }
}
