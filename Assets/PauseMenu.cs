using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static PauseMenu main;

    private float prevTimeScale;
    private bool open = false;
    public bool Open
    {
        get
        {
            return open;
        }
        set
        {
            open = value;
            foreach (Transform child in transform)
                child.gameObject.SetActive(open);
            if (open)
            {
                prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = prevTimeScale;
            }
        }
    }

    void Awake()
    {
        main = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Open = !Open;
    }

    public void Resume()
    {
        Open = false;
    }

    public void Options()
    {
        // TODO
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
