using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public Login login;
    public InputField userField;
    public InputField passwordField;
    public InputField confirmPasswordField;
    public string error;

    public void AttemptRegisterUser()
    {
        if (passwordField.text.Trim() == confirmPasswordField.text.Trim())
            StartCoroutine(RegisterCoroutine());
        else
            Debug.Log("Passwords did not match.");
    }

    IEnumerator RegisterCoroutine()
    {
        Dictionary<string, string> formData = new Dictionary<string, string>()
        {
            { "username", userField.text.Trim() },
            { "password", passwordField.text.Trim() }
        };
        UnityWebRequest www = UnityWebRequest.Post(WebServer.SERVER + "/register", formData);
        yield return www.Send();
        if (www.isError)
        {
            Debug.Log("Error registering: " + www.error);
            error = www.error;
        }
        else if (www.responseCode >= 400)
        {
            Debug.Log("Failed login");
        }
        else
        {
            PostRegister();
        }
    }

    void PostRegister()
    {
        gameObject.SetActive(false);
        login.gameObject.SetActive(true);
        login.userField.text = userField.text;
        login.passwordField.text = passwordField.text;
        login.AttemptLogin();
    }
}
