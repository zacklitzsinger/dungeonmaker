using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SearchBar : MonoBehaviour
{

    public Transform content;
    InputField field;

    private void Start()
    {
        field = GetComponent<InputField>();
    }

    public void Search()
    {
        string searchTerm = field.text.Trim();
        foreach (LevelInfo levelInfo in content.GetComponentsInChildren<LevelInfo>(true))
        {
            bool active = Path.GetFileNameWithoutExtension(levelInfo.LevelName).Trim().Contains(searchTerm);
            active |= levelInfo.Author.Trim().Contains(searchTerm);
            levelInfo.gameObject.SetActive(active);
        }
    }

}
