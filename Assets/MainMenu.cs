using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelBrowser;

    public void Play()
    {
        mainMenu.SetActive(false);
        levelBrowser.SetActive(true);
    }

    public void Create()
    {
        SceneManager.LoadScene("LevelEditor");
    }

}
