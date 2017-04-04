using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour {

    public InputField userField;
    public InputField passwordField;
    public string error;

    private void Start()
    {
        passwordField.onEndEdit.AddListener((value) =>
        {
            if (Input.GetKey(KeyCode.Return))
                AttemptLogin();
        });
    }

    public void AttemptLogin()
    {
        StartCoroutine(LoginCoroutine());
    }

    IEnumerator LoginCoroutine()
    {
        Dictionary<string, string> formData = new Dictionary<string, string>()
        {
            { "username", userField.text.Trim() },
            { "password", passwordField.text.Trim() }
        };
        UnityWebRequest www = UnityWebRequest.Post(WebServer.SERVER + "/login", formData);
        yield return www.Send();
        if (www.isError)
        {
            Debug.Log("Error logging in: " + www.error);
            error = www.error;
        }
        else if (www.responseCode >= 400)
        {
            Debug.Log("Failed login");
        }
        else
        {
            WebServer.COOKIE = www.GetResponseHeader("set-cookie");
            SceneManager.LoadScene("MainMenu");
        }
    }

}
