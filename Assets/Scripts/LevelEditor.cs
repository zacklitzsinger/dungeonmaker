﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EditMode
{
    Test,
    Edit,
    Circuit
}

public class LevelEditor : MonoBehaviour, ICustomSerializable
{
    public EditMode mode = EditMode.Edit;
    [ReadOnly]
    public GameObject selectedPrefab;
    public GameObject selectedGameObject;
    public GameObject[] prefabOptions;
    public int gridX = 32, gridY = 32;

    // UI
    public GameObject sidebar;
    public InputField levelNameInput;
    public Text editModeLabel;
    public GameObject prefabButton;
    public GameObject sidebarContent;
    public Texture selectionBox;

    public string levelName;
    public Dictionary<Vector2, List<GameObject>> tilemap = new Dictionary<Vector2, List<GameObject>>();

    void Start()
    {
        foreach (GameObject option in prefabOptions)
        {
            GameObject button = Instantiate(prefabButton, sidebarContent.transform);
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

        levelNameInput.onValueChanged.AddListener((string str) =>
        {
            levelName = str;
        });
    }

    // Update is called once per frame
    void Update()
    {
        // Allow cycling of edit mode
        if (Input.GetButtonDown("Edit"))
        {
            mode = (EditMode)(((int)mode + 1) % Enum.GetNames(typeof(EditMode)).Length);
        }

        // Pause time while editing
        Time.timeScale = (mode >= EditMode.Edit ? 0 : 1);

        editModeLabel.text = mode.ToString();

        // Hide editing sidebar while editing
        sidebar.SetActive(mode >= EditMode.Edit);
        sidebarContent.SetActive(mode == EditMode.Edit);

        if (EventSystem.current.IsPointerOverGameObject() || mode == EditMode.Test)
            return;

        switch (mode)
        {
            case EditMode.Edit:

                // Allow placing of objects by left clicking
                if (Input.GetMouseButton(0) && selectedPrefab != null)
                {
                    ObjectData data = selectedPrefab.GetComponent<ObjectData>();
                    if (data == null)
                        throw new Exception("Selected prefab missing ObjectData");
                    Vector2 pos = GetGridMousePosition();
                    if (!tilemap.ContainsKey(pos))
                        tilemap[pos] = new List<GameObject>();
                    if (tilemap[pos].Exists((o) => { return o.GetComponent<ObjectData>().type == data.type; }))
                        return;
                    GameObject go = CreateObjectAtGrid(pos, selectedPrefab);
                    tilemap[pos].Add(go);
                }

                // Allow removal of objects by right clicking
                if (Input.GetMouseButton(1))
                {
                    Vector2 mouseGridPos = GetGridMousePosition();
                    Collider2D[] colliders = Physics2D.OverlapPointAll(mouseGridPos);
                    foreach (Collider2D collider in colliders)
                    {
                        if (collider.transform.parent != transform)
                            continue;
                        tilemap.Remove(mouseGridPos);
                        Destroy(collider.gameObject);
                    }
                }
                break;

            case EditMode.Circuit:

                if (Input.GetMouseButtonDown(0))
                {
                    GameObject go = GetGameObjectAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    if (go.GetComponent<Circuit>())
                    {
                        selectedGameObject = go;
                    }
                }
                if (Input.GetMouseButtonUp(0) && selectedGameObject)
                {
                    GameObject go = GetGameObjectAtPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    if (go)
                    {
                        Circuit circuit = go.GetComponent<Circuit>();
                        Circuit otherCircuit = selectedGameObject.GetComponent<Circuit>();
                        if (circuit)
                        {
                            circuit.Connect(otherCircuit);
                            otherCircuit.Connect(circuit);
                            selectedGameObject = null;
                        }
                    }
                    else
                    {
                        selectedGameObject = null;
                    }
                }
                break;
        }
    }

    Vector2 GetGridMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(ConvertPositionToGrid(Input.mousePosition));
    }

    GameObject CreateObjectAtGrid(Vector2 point, GameObject obj)
    {
        GameObject newObj = Instantiate(obj, point, Quaternion.identity, transform);
        newObj.name = obj.name;
        return newObj;
    }

    GameObject GetGameObjectAtPoint(Vector2 point)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(point);
        foreach (Collider2D collider in colliders)
        {
            if (collider.transform.parent != transform)
                continue;
            return collider.gameObject;
        }
        return null;
    }

    void OnGUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (mode == EditMode.Edit)
        {
            // Draw currently selected grid square
            Vector3 pos = ConvertPositionToGrid(Input.mousePosition);
            pos.y = Screen.height - pos.y;
            pos.x -= gridX / 2;
            pos.y -= gridY / 2;
            GUI.DrawTexture(new Rect(pos, new Vector2(gridX, gridY)), selectionBox);
        }

        if (mode == EditMode.Circuit)
        {
            if (selectedGameObject)
            {
                Line line = Camera.main.GetComponent<Line>();
                List<Vector2> points = new List<Vector2>();
                points.Add(selectedGameObject.transform.position);
                points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                line.DrawLine(points, Color.red);
            }
        }
    }

    public void SaveToDisk()
    {
        if (!Directory.Exists("Levels"))
        {
            Directory.CreateDirectory("Levels");
        }
        string filename = Path.Combine("Levels", levelName);
        FileStream fstream = File.Open(filename, FileMode.Create);
        using (BinaryWriter bw = new BinaryWriter(fstream))
            Serialize(bw);
    }

    public void LoadFromDisk()
    {
        FileStream fstream = File.Open(Path.Combine("Levels", levelName), FileMode.Open);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        tilemap.Clear();
        using (BinaryReader br = new BinaryReader(fstream))
            Deserialize(br);
    }

    Vector3 ConvertPositionToGrid(Vector3 pos)
    {
        pos.x = pos.x + gridX / 2 - pos.x % gridX;
        pos.y = pos.y + gridY / 2 - pos.y % gridY;
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
                SerializeObject(go, bw);
            }
        }
    }

    void SerializeObject(GameObject go, BinaryWriter bw)
    {
        bw.Write(go.name);
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
        int tileCount = br.ReadInt32();
        for (int i = 0; i < tileCount; i++)
        {
            Vector2 pos = new Vector2(br.ReadSingle(), br.ReadSingle());
            int goCount = br.ReadInt32();
            List<GameObject> goList = new List<GameObject>();
            tilemap[pos] = goList;
            for (int j = 0; j < goCount; j++)
            {
                string goName = br.ReadString();
                GameObject prefab = Array.Find(prefabOptions, (o) => { return o.name == goName; });
                if (prefab == null)
                {
                    throw new Exception("Could not find prefab in level named " + goName);
                }
                GameObject go = CreateObjectAtGrid(pos, prefab);
                goList.Add(go);
                DeserializeObject(go, br);
            }
        }
    }


    void DeserializeObject(GameObject go, BinaryReader br)
    {
        // Skip reading the game object name because it's what gets instantiated
        var count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            var type = br.ReadString();
            var s = (ICustomSerializable)go.GetComponent(type);
            s.Deserialize(br);
        }
    }
}
