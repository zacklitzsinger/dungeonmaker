using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
    Animator animator;

    public Text nameField;
    public Text authorField;
    public Text dateField;
    public Text descriptionField;
    public Text sizeField;
    public Toggle topSection;
    public Button playButton;

    public bool local;
    private string levelName;
    public string LevelName
    {
        get
        {
            return levelName;
        }

        set
        {
            levelName = value;
            nameField.text = Path.GetFileNameWithoutExtension(value);
        }
    }

    private string author;
    public string Author
    {
        get
        {
            return author;
        }

        set
        {
            author = value;
            authorField.text = value;
        }
    }

    private DateTime lastUpdateDate;
    public DateTime LastUpdateDate
    {
        get
        {
            return lastUpdateDate;
        }

        set
        {
            lastUpdateDate = value;
            dateField.text = value.ToShortTimeString();
        }
    }

    private string description;
    public string Description
    {
        get
        {
            return description;
        }

        set
        {
            description = value;
            descriptionField.text = value;
        }
    }

    private int size;
    public int Size
    {
        get
        {
            return size;
        }

        set
        {
            size = value;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int len = value;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            sizeField.text = String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LevelPlay");
            LevelLoader.main.StartCoroutine(local ? LevelLoader.main.LoadLevelFromLocal(levelName) : LevelLoader.main.LoadLevelFromWeb(levelName));
        });
    }

    void Update()
    {
        animator.SetBool("open", topSection.isOn);
    }
}
