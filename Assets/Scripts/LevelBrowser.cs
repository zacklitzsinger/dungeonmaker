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
        WWW www = new WWW("localhost:3000/levels");
        yield return www;
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }
        JSONNode json = JSON.Parse(www.text);
        foreach(JSONNode info in json.Children)
        {
            string levelName = info["name"];
            GameObject infoPanel = Instantiate(levelInfo, content);
            infoPanel.GetComponentInChildren<Text>().text = levelName;
            infoPanel.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LevelPlay");
                StartCoroutine(LoadLevelFromWeb(levelName));
            });
        }
    }

    IEnumerator LoadLevelFromWeb(string levelName)
    {
        WWW www = new WWW("localhost:3000/levels/download/" + levelName);
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
