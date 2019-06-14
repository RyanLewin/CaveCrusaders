using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static bool GamePaused { get; set; }
    // [SerializeField] SoundManager _soundManager;


    void Start()
    {
        ResumeGame();
    }

    /// <summary>
    /// Go to game scene
    /// </summary>
    public void Play(string levelName)
    {
        //  _soundManager.ButtonClick();
        LoadLevelSceneScript.instance.StartLoadGameLevel(levelName);
    }

    public void PlayNext()
    {
        //Workaround for unity bug where getting scene name after getting scene by build index returns empty string
        // _soundManager.ButtonClick();
        string pathToScene = SceneUtility.GetScenePathByBuildIndex(SceneManager.GetSceneByName(StaticMapInfo.Level).buildIndex + 1);
        string nextLevelName = System.IO.Path.GetFileNameWithoutExtension(pathToScene);
        LoadLevelSceneScript.instance.StartLoadGameLevel(nextLevelName);
    }

    /// <summary>
    /// Go to level select scene
    /// </summary>
    public void LevelSelect()
    {
        // _soundManager.ButtonClick();
        StaticMapInfo.LoadingIntoLevelEditor = false;
        StartCoroutine(FadeLoad("LevelSelect"));
    }

    /// <summary>
    /// Go to level editor scene
    /// </summary>
    public void LevelEditor()
    {
        // _soundManager.ButtonClick();
        StaticMapInfo.LoadingIntoLevelEditor = true;
        StaticMapInfo.SetLevelLoadData("", false);
        StaticMapInfo.LevelEditorMode = WorldController.CurrentGameMode.Editor;
        StartCoroutine(FadeLoad("Master"));
    }

    /// <summary>
    /// Go to settings scene
    /// </summary>
    public void Settings()
    {
        // _soundManager.ButtonClick();
        StartCoroutine(FadeLoad("Settings"));
    }

    /// <summary>
    /// Go to main menu scene
    /// </summary>
    public void MainMenu()
    {
        // StartCoroutine(FadeLoad("MainMenu"));
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Go to credtis scene
    /// </summary>
    public void Credits()
    {
        //_soundManager.ButtonClick();
        StartCoroutine(FadeLoad("Credits"));
    }

    /// <summary>
    /// Quit application
    /// </summary>
    public void Quit()
    {
        // _soundManager.ButtonClick();
        Application.Quit();
    }

    /// <summary>
    /// Reload the current level
    /// </summary>
    public void RestartLevel()
    {
        // _soundManager.ButtonClick();
        ResumeGame();
        LoadLevelSceneScript.instance.StartLoadGameLevel(StaticMapInfo.Level);
    }

    /// <summary>
    /// Set timescale to 0
    /// </summary>
    public void PauseGame()
    {
        //  _soundManager.ButtonClick();
        Time.timeScale = 0f;
        GamePaused = true;
    }

    /// <summary>
    /// Set timescale to 1
    /// </summary>
    public void ResumeGame()
    {
        if (WorldController.GetWorldController != null)
        {

            if (WorldController.GetWorldController._soundManager != null)
            {
                WorldController.GetWorldController._soundManager.WinLossReturnToGame();
            }
        }
        Time.timeScale = 1f;
        GamePaused = false;
    }

    IEnumerator FadeLoad(string loadName)
    {
        yield return StartCoroutine(FadeScript.instance.FadeOut());
        SceneManager.LoadScene(loadName);
    }
}
