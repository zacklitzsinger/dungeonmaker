using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.IO;

public class LevelBrowser : MonoBehaviour {

    public GameObject levelInfo;
    public Transform content;

	// Use this for initialization
	void Start () {
        
        DontDestroyOnLoad(gameObject);
	}
	
    public void LoadLevelList()
    {
        foreach (string filename in Directory.GetFiles(Application.persistentDataPath))
        {
            GameObject infoPanel = Instantiate(levelInfo, content);
            infoPanel.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(filename);
            infoPanel.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LevelPlay");
                StartCoroutine(LoadLevelFromLocal(filename));
            });
        }
        StartCoroutine(FetchLevelInfo());
    }

    IEnumerator FetchLevelInfo() {
        WWW www = new WWW(WebServer.SERVER + "/levels");
        yield return www;
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }
        JSONNode json = JSON.Parse(www.text);
        foreach(JSONNode info in json.Children)
        {
            GameObject infoPanel = Instantiate(levelInfo, content);
            foreach (Text text in infoPanel.GetComponentsInChildren<Text>())
            {
                switch (text.name) {
                    case "Name":
                        text.text = info["name"];
                        break;
                    case "Description":
                        text.text = info["description"];
                        break;
                    case "Size":
                        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                        int len = info["size"].AsInt;
                        int order = 0;
                        while (len >= 1024 && order < sizes.Length - 1)
                        {
                            order++;
                            len = len / 1024;
                        }

                        // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                        // show a single decimal place, and no space.
                        string result = String.Format("{0:0.##} {1}", len, sizes[order]);
                        text.text = result;
                        break;
                }
            }
            infoPanel.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LevelPlay");
                StartCoroutine(LoadLevelFromWeb(info["name"]));
            });
        }
    }

    IEnumerator LoadLevelFromWeb(string levelName)
    {
        WWW www = new WWW(WebServer.SERVER + "/levels/download/" + levelName);
        yield return www;
        if (www.error != null)
            Debug.LogError(www.error);
        LevelEditor.main.ChangeMode(EditMode.Play);
        LevelEditor.main.LoadFromBytes(www.bytes);
    }

    IEnumerator LoadLevelFromLocal(string levelName)
    {
        while (LevelEditor.main == null)
            yield return new WaitForEndOfFrame();
        LevelEditor.main.LoadFromDisk(levelName);
    }
}
