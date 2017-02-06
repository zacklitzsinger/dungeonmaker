using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour
{

    public bool editing = true;
    public GameObject selectedPrefab;
    public GameObject[] prefabOptions;
    public GameObject prefabButton;
    public GameObject buttonParent;
    public Texture selectionBox;
    public int gridX = 32, gridY = 32;

    public LevelInfo levelData = new LevelInfo();
    public InputField levelNameInput;
    public BinaryFormatter bf = new BinaryFormatter();

    void Start()
    {
        foreach (GameObject option in prefabOptions)
        {
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

        levelNameInput.onValueChanged.AddListener((string str) =>
        {
            levelData.name = str;
        });

        // Set up binary formatter
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector2SerializationSurrogate vector2SS = new Vector2SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), vector2SS);
        bf.SurrogateSelector = surrogateSelector;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Edit"))
            editing = !editing;
        if (editing)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
        if (EventSystem.current.IsPointerOverGameObject() || !editing)
            return;
        if (Input.GetMouseButton(0) && editing && selectedPrefab != null)
        {
            ObjectInfo info = selectedPrefab.GetComponent<ObjectData>().info;
            Vector2 pos = Camera.main.ScreenToWorldPoint(ConvertPositionToGrid(Input.mousePosition));
            if (!levelData.tilemap.ContainsKey(pos))
                levelData.tilemap[pos] = new List<ObjectInfo>();
            if (levelData.tilemap[pos].Exists((o) => { return o.type == info.type; }))
                return;
            CreateObjectAtGrid(pos, selectedPrefab);
            levelData.tilemap[pos].Add(info);
        }
        if (Input.GetMouseButton(1))
        {
            Vector2 mouseGridPos = Camera.main.ScreenToWorldPoint(ConvertPositionToGrid(Input.mousePosition));
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseGridPos);
            foreach(Collider2D collider in colliders)
            {
                if (collider.transform.parent != transform)
                    continue;
                levelData.tilemap.Remove(mouseGridPos);
                Destroy(collider.gameObject);
            }
        }
    }

    GameObject CreateObjectAtGrid(Vector2 point, GameObject obj)
    {
        GameObject newObj = Instantiate(obj, point, Quaternion.identity, transform);
        newObj.name = obj.name;
        return newObj;
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

    public void SaveToDisk()
    {
        if (!Directory.Exists("Levels"))
        {
            Directory.CreateDirectory("Levels");
        }
        string filename = Path.Combine("Levels", levelData.name);
        FileStream fstream = File.Open(filename, FileMode.Create);
        bf.Serialize(fstream, levelData);
        fstream.Close();
    }

    public void LoadFromDisk()
    {
        FileStream fstream = File.Open(Path.Combine("Levels", levelData.name), FileMode.Open);
        levelData = (LevelInfo)bf.Deserialize(fstream);
        fstream.Close();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (KeyValuePair<Vector2, List<ObjectInfo>> pair in levelData.tilemap)
            foreach (ObjectInfo info in pair.Value)
            {
                string tileName = info.name;
                GameObject go = Array.Find<GameObject>(prefabOptions, (g) => { return g.name == tileName; });
                if (go == null)
                {
                    Debug.LogWarning("Could not find game object named: " + tileName);
                    continue;
                }
                GameObject newObj = CreateObjectAtGrid(pair.Key, go);
                newObj.GetComponent<ObjectData>().info = info;
            }
    }

    Vector3 ConvertPositionToGrid(Vector3 pos)
    {
        pos.x = pos.x + gridX / 2 - pos.x % gridX;
        pos.y = pos.y + gridY / 2 - pos.y % gridY;
        return pos;
    }
}
