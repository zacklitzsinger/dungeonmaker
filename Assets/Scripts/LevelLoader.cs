using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LevelLoader : MonoBehaviour {

    public static LevelLoader main;

	// Use this for initialization
	void Awake() {
        main = this;
        DontDestroyOnLoad(gameObject);
	}

    public IEnumerator LoadLevelFromWeb(string levelName)
    {
        UnityWebRequest www = UnityWebRequest.Get(WebServer.SERVER + "/levels/download/" + levelName);
        www.SetRequestHeader("Cookie", WebServer.COOKIE);
        yield return www.Send();
        if (www.isError)
        {
            Debug.LogError(www.error);
            yield break;
        }
        while (LevelEditor.main == null)
            yield return new WaitForEndOfFrame();
        LevelEditor.main.ChangeMode(EditMode.Play);
        LevelEditor.main.LoadFromBytes(www.downloadHandler.data);
    }

    public IEnumerator LoadLevelFromLocal(string levelName)
    {
        while (LevelEditor.main == null)
            yield return new WaitForEndOfFrame();
        LevelEditor.main.LoadFromDisk(levelName);
    }
}
