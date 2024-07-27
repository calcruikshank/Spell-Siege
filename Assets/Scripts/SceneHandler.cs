using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }
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
    public void LoadGameScene()
    {
        SceneManager.LoadSceneAsync("SimpleGame");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
