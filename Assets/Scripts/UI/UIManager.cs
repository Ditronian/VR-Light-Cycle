//Code in this file was written whilst following along with Tom Weiland's C# Networking Tutorial.  Link below.
//https://www.youtube.com/watch?v=uh8XaC0Y5MA&list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5&index=1

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipField;
    public InputField portField;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Int32.TryParse(portField.text, out int port);
        Client.instance.ConnectToServer(ipField.text, port);
        SceneManager.LoadScene("Grid");
    }

}
