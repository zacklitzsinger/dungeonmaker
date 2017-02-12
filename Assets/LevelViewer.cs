using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LevelViewer : MonoBehaviour {

    public Button buttonPrefab;
    public Transform content;

	// Use this for initialization
	void Start () {

        foreach (string filename in Directory.GetFiles(Application.persistentDataPath))
        {
            Button button = Instantiate(buttonPrefab, content);
            button.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(filename);
            button.onClick.AddListener(() =>
            {
                LevelEditor.main.LoadFromDisk(filename);
                gameObject.SetActive(false);
            });
        }
	}

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

}
