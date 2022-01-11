using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;
    public InputField ipField;
    public GameObject eolMenu;
    public TextMeshProUGUI keyCounterLabel;

    /*
     * On awake, set UI manager instance to this 
     */
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object.");
            Destroy(this);
        }
    }

    /*
     * Method called to connect a player to the server 
     * Runs when the player presses the 'Connect' UI button and has entered something in the username input field 
     */
    public void ConnectToServer()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            return;
        }
        else
        {
            startMenu.SetActive(false);
            usernameField.interactable = false;
            Client.instance.ConnectToServer();
        }
        
    }

    /*
     * Method to activate the end of level UI screen upon completion of the level 
     */
    public void EndOfLevelUI()
    {
        eolMenu.SetActive(true);
    }

    /*
     * Method to quit the application
     * Runs when the 'Quit' button is pressed on the end of level UI screen 
     */
    public void QuitBtnPressed()
    {
        Application.Quit();
        
        // Only include this line of code if running in the editor 
        //EditorApplication.ExitPlaymode();
    }
}
