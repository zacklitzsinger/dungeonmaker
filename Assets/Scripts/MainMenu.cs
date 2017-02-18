using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public LevelBrowser levelBrowser;

    public void Play()
    {
        mainMenu.SetActive(false);
        levelBrowser.gameObject.SetActive(true);
        levelBrowser.LoadLevelList();
    }

    public void Create()
    {
        SceneManager.LoadScene("LevelEditor");
    }

}
