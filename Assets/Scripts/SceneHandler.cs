using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
   
    public void LoadLobby()
    {
        SceneManager.LoadSceneAsync("Lobby");
    }
    public void LoadCollection()
    {
        SceneManager.LoadSceneAsync("CardCollection");
    }
    public void LoadPacks()
    {
        SceneManager.LoadSceneAsync("Gamble");
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
