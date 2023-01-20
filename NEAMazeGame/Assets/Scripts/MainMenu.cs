using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI gameOverInfoText;

    void Start(){
        if(gameOverInfoText != null){
            // save final stage and score in variables
            int finalStage = PlayerPrefs.GetInt("CurrentStage");
            int finalScore = PlayerPrefs.GetInt("RecentScore");

            // format game over text to display score and stage
            gameOverInfoText.text = "All Players have died.\n\nStage Reached: " + 
            finalStage + "\nTotal Score: " + finalScore + " x " + finalStage + " = "  + 
            (finalScore * finalStage) + "\n\nWill you try again?";
        }
        // reset score and stage prefs
        PlayerPrefs.SetInt("CurrentStage", 1);
        PlayerPrefs.SetInt("RecentScore", 0);
    }
    
    public void loadLobbyFinderScene(){
        SceneManager.LoadScene(1);
        Debug.Log("Lobby Finder scene has been loaded");
    }

    public void loadSettingsScene(){
        SceneManager.LoadScene(2);
        Debug.Log("Settings scene has been loaded");
    }

    public void loadAboutScene(){
        SceneManager.LoadScene(3);
        Debug.Log("About scene has been loaded");
    }

    public void quitGame(){
        Debug.Log("Application closed successfully");
        Application.Quit();
    }

    public void returnToMenu(){
        SceneManager.LoadScene(0);
        Debug.Log("Main menu scene has been loaded");
        PlayerPrefs.SetInt("CurrentStage", 1);
        PlayerPrefs.SetInt("RecentScore", 0);
    }

    public void toggleFullscreen(){
        Screen.fullScreen = !Screen.fullScreen;
        if(Screen.fullScreen){
            Debug.Log("Fullscreen mode has been turned on");
        } else {
            Debug.Log("Fullscreen mode has been turned off");
        }
    }
}

