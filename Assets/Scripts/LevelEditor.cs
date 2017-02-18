using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EditMode
{
    Inactive,
    Test,
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
    [ReadOnly]
    public GameObject selectedPrefab;
    [ReadOnly]
    public GameObject selectedGameObject;
    public PrefabGroup[] prefabGroups;
    private GameObject[] allPrefabOptions;
    [ReadOnly]
    public float rotation;
    public const int GRID_SIZE = 32;

    // UI
    public GameObject sidebar;
    public InputField levelNameInput;
    public Text editModeLabel;
    public GameObject prefabButton;
    public GameObject sidebarContent;
    public Texture selectionBox;
    public Toggle prefabToggle;
    public GameObject prefabIntSlider;
    public ToggleGroup tabPanel;
    public Toggle prefabToggleButton;

    // Circuits
    public Color circuitColor;
    public Color selectedCircuitInputColor;
    public Color selectedCircuitOutputColor;
    public Color selectedConnectionColor;

    // Level information
    public string levelName;
    public Dictionary<Vector2, List<GameObject>> tilemap = new Dictionary<Vector2, List<GameObject>>();
    public Dictionary<Guid, GameObject> guidmap = new Dictionary<Guid, GameObject>();
    public NavMap navmap;
    public NavigationCalculator<MapNode> navcalc;
    List<MapNode> currentRoom = new List<MapNode>();

    /// <summary>
    /// When testing, save to a temporary file beforehand so we can reload the level after finishing
    /// </summary>
    private string tempFilename;
    Vector2 lastMousePosition;
    /// <summary>
    /// Was the app focused last frame? If false, app just became focused, so we should ignore many UI inputs.
    /// </summary>
    bool lastAppFocused = true;

    void Start()
    {
        main = this;
        navmap = new NavMap(tilemap);
        navcalc = new NavigationCalculator<MapNode>(navmap);

        List<GameObject> prefabs = new List<GameObject>();
        foreach (PrefabGroup group in prefabGroups)
            prefabs.AddRange(group.prefabs);
        allPrefabOptions = prefabs.ToArray();

        SidebarCreateButtons();

        if (levelNameInput != null)
            levelNameInput.onValueChanged.AddListener((string str) =>
            {
                levelName = str;
            });
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
        if (mode == EditMode.Create)
            SidebarCreateButtons();
        if (mode == EditMode.Test)
        {
            SaveToTemp();
        }
        if (prevMode == EditMode.Test)
        {
            currentRoom.Clear();
            LoadFromTemp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == EditMode.Inactive)
            return;
        if (Input.GetButtonDown("Create Mode"))
            ChangeMode(EditMode.Create);
        else if (Input.GetButtonDown("Edit Mode"))
            ChangeMode(EditMode.Edit);
        else if (Input.GetButtonDown("Circuit Mode"))
            ChangeMode(EditMode.Circuit);
        else if (Input.GetButtonDown("Test Mode"))
            ChangeMode(EditMode.Test);
        if (Input.GetButtonDown("Rotate CW"))
            rotation = (rotation + 90f) % 360;
        else if (Input.GetButtonDown("Rotate CCW"))
            rotation = (rotation + 270f) % 360;

        // Pause time while editing
        Time.timeScale = (mode >= EditMode.Create ? 0 : 1);

        if (editModeLabel != null)
            editModeLabel.text = mode.ToString();

        // Hide editing sidebar while editing
        if (sidebar != null)
            sidebar.SetActive(mode >= EditMode.Create);

        if (EventSystem.current.IsPointerOverGameObject() || mode == EditMode.Test)
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
                    }

                    // Allow removal of objects by right clicking
                    if (Input.GetMouseButton(1))
                    {
                        foreach (Vector2 point in GetGridPointsAlongLine(lastMousePosition, Input.mousePosition))
                            DestroyAllGameObjectsAtGridPosition(point);
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
                        Debug.Log("Selected game object: " + go.name);
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
            tilemap[gridPos] = new List<GameObject>();
        GameObject sameGroupGameObject = tilemap[gridPos].Find((o) => { return o.GetComponent<ObjectData>().type == data.type; });
        if (sameGroupGameObject)
        {
            // Don't replace if it's the same exact type of object
            if (sameGroupGameObject.name == selectedPrefab.name)
                return null;
            DestroyGameObjectAtGridPosition(gridPos, sameGroupGameObject);
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
        List<GameObject> goList = tilemap[gridPos];
        while (goList.Count > 0)
            DestroyGameObjectAtGridPosition(gridPos, goList[0]);
    }

    /// <summary>
    /// Destroy a single game object at the grid position in the tilemap.
    /// </summary>
    void DestroyGameObjectAtGridPosition(Vector2 gridPos, GameObject go)
    {
        if (!tilemap.ContainsKey(gridPos))
            return;
        ObjectData data = go.GetComponent<ObjectData>();
        List<GameObject> goList = tilemap[gridPos];
        goList.Remove(go);
        if (goList.Count == 0)
            tilemap.Remove(gridPos);
        Guid id = data.guid;
        guidmap.Remove(id);
        Destroy(go);
    }

    /// <summary>
    /// Returns the grid position under the mouse.
    /// </summary>
    /// <returns></returns>
    Vector2 GetGridMousePosition()
    {
        return ConvertPositionToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    /// <summary>
    /// Instantiates given prefab at the point on the grid
    /// </summary>
    GameObject CreateObjectAtGrid(Vector2 point, GameObject obj)
    {
        GameObject newObj = Instantiate(obj, point, Quaternion.identity, transform);
        newObj.name = obj.name;
        if (!tilemap.ContainsKey(point))
            tilemap[point] = new List<GameObject>();
        tilemap[point].Add(newObj);
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
        foreach (GameObject go in tilemap[point])
        {
            // Must be an object the level editor knows about
            if (go.transform.parent != transform)
                continue;
            if (choice == null)
                choice = go.GetComponent<ObjectData>();
            else if (choice.type < go.GetComponent<ObjectData>().type)
                choice = go.GetComponent<ObjectData>();
        }
        return choice ? choice.gameObject : null;
    }

    /// <summary>
    /// Set the currently active room. Uses NavMap to determine what is contained in the current room.
    /// </summary>
    public void SetCurrentRoom(Vector2 gridPos)
    {
        gridPos = ConvertPositionToGrid(gridPos);
        MapNode currentNode = new MapNode(gridPos);
        if (currentRoom.Contains(currentNode))
            return;
        currentRoom = navcalc.GetConnectedNodes(currentNode, true);
        // Iterate only through the original list.
        int nodeCount = currentRoom.Count;
        for (int i = 0; i < nodeCount; i++)
        {
            foreach (MapNode node in navmap.GetPotentialNeighbors(currentRoom[i]))
                if (!currentRoom.Contains(node))
                    currentRoom.Add(node);
        }

        // Update visibility
        foreach (KeyValuePair<Vector2, List<GameObject>> pair in tilemap)
        {
            bool active = currentRoom.Contains(new MapNode(pair.Key));
            foreach (GameObject go in pair.Value)
                if (go != null)
                    SetTileActive(go, go.tag == "Player" || active);
        }

        // Update camera
        Vector2 avg = Vector2.zero;
        foreach (MapNode node in currentRoom)
            avg += new Vector2(node.x, node.y);
        avg /= currentRoom.Count;
        Camera.main.GetComponent<CameraFollow>().SetTarget(avg);
    }

    /// <summary>
    /// Shows or hides a given tile (based on whether it is in the current room).
    /// </summary>
    void SetTileActive(GameObject go, bool active)
    {
        if (go == null)
            return;
        StartCoroutine(ControlAlpha(go.GetComponent<SpriteRenderer>(), active ? 1f : 0f));
        foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>())
            ps.gameObject.SetActive(active);
    }

    IEnumerator ControlAlpha(SpriteRenderer r, float targetAlpha)
    {
        float alpha = r.color.a;
        while (alpha != targetAlpha && r != null)
        {
            alpha = Mathf.Lerp(alpha, targetAlpha, 0.15f);
            if (Math.Abs(alpha - targetAlpha) < 0.05f)
                alpha = targetAlpha;
            Color c = r.color;
            c.a = alpha;
            r.color = c;
            yield return new WaitForFixedUpdate();
        }
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
                }
            }
        }
    }

    void OnGUI()
    {
        switch (mode)
        {
            case EditMode.Create:
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                // Draw currently selected grid square
                Vector2 gridPos = Camera.main.WorldToScreenPoint(ConvertPositionToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
                gridPos.y = Screen.height - gridPos.y;
                gridPos -= Vector2.one * GRID_SIZE / 2;
                GUI.DrawTexture(new Rect(gridPos, new Vector2(GRID_SIZE, GRID_SIZE)), selectionBox);
                if (selectedPrefab)
                {
                    Sprite sprite = selectedPrefab.GetComponent<SpriteRenderer>().sprite;
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
                    Vector2 rectPoint = Camera.main.WorldToScreenPoint((Vector2)selectedGameObject.transform.position);
                    rectPoint.y = Screen.height - rectPoint.y;
                    GUI.DrawTexture(new Rect(rectPoint - Vector2.one * GRID_SIZE / 2, new Vector2(GRID_SIZE, GRID_SIZE)), selectionBox);
                }

                break;

            case EditMode.Circuit:
                Line line = Camera.main.GetComponent<Line>();
                // Draw line from selected object to mouse if we are placing a circuit
                if (selectedGameObject)
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
        form.AddBinaryData("level", File.ReadAllBytes(filename));
        WWW www = new WWW("localhost:3000/levels", form);
        yield return www;
        if (www.error != null)
            Debug.Log(www.error);
        else
            Debug.Log("Finished uploading level");
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
    public Vector3 ConvertPositionToGrid(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x);
        pos.y = Mathf.Round(pos.y);
        return pos;
    }

    public void Serialize(BinaryWriter bw)
    {
        bw.Write(levelName);
        bw.Write(tilemap.Count);
        foreach (KeyValuePair<Vector2, List<GameObject>> pair in tilemap)
        {
            bw.Write(pair.Key.x);
            bw.Write(pair.Key.y);
            bw.Write(pair.Value.Count);
            foreach (GameObject go in pair.Value)
            {
                bw.Write(go.name);
                Guid id = go.GetComponent<ObjectData>().guid;
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
        if (levelNameInput != null)
            levelNameInput.text = levelName;
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
            DeserializeComponents(go, br);
        }

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
