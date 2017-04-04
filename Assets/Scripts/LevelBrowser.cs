using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LevelBrowser : MonoBehaviour
{

    public LevelInfo levelDisplayPrefab;
    public Transform content;

    public void LoadLevelList()
    {
        foreach (string filename in Directory.GetFiles(Application.persistentDataPath))
        {
            LevelInfo infoPanel = Instantiate(levelDisplayPrefab, content);
            infoPanel.local = true;
            infoPanel.LevelName = filename;
            infoPanel.topSection.group = content.GetComponent<ToggleGroup>();
        }
        StartCoroutine(FetchLevelInfo());
    }

    IEnumerator FetchLevelInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(WebServer.SERVER + "/levels");
        www.SetRequestHeader("Cookie", WebServer.COOKIE);
        yield return www.Send();
        if (www.isError || www.responseCode >= 400)
        {
            Debug.LogError(www.error);
            yield break;
        }
        JSONNode json = JSON.Parse(www.downloadHandler.text);
        List<LevelInfo> levelInfos = new List<LevelInfo>();
        foreach (JSONNode info in json.Children)
        {
            LevelInfo infoPanel = Instantiate(levelDisplayPrefab);
            levelInfos.Add(infoPanel);
            infoPanel.local = false;
            infoPanel.LevelName = info["name"];
            infoPanel.Description = info["description"];
            infoPanel.Size = info["size"].AsInt;
            infoPanel.Author = info["author"];
            infoPanel.LastUpdateDate = DateTime.Parse(info["utime"]);
            infoPanel.topSection.group = content.GetComponent<ToggleGroup>();
        }
        levelInfos.Sort((a, b) => { return a.LastUpdateDate.CompareTo(b.LastUpdateDate); });
        foreach (LevelInfo info in levelInfos)
            info.transform.SetParent(content);
    }
}
