using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LoadLevelSceneScript : MonoBehaviour
{
    public static LoadLevelSceneScript instance;

    [SerializeField]
    GameObject _levelLoadingUI;
    [SerializeField]
    Text _loadingText;

    //bool _isLoading = false;

    private void Awake()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        if (instance == null)
        {
            instance = this;
        }
    }

    public void StartLoadGameLevel(string levelName)
    {
        StartCoroutine(LoadGameLevel(levelName));
    }

    IEnumerator LoadGameLevel(string levelName)
    {
        yield return StartCoroutine(FadeScript.instance.FadeOut());
        FadeScript.instance.gameObject.SetActive(false);
        _levelLoadingUI.SetActive(true);

        StaticMapInfo.SetLevelLoadData(levelName, true);
        AsyncOperation asyncMaster = SceneManager.LoadSceneAsync("Master");
        AsyncOperation asyncLevel = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        asyncMaster.allowSceneActivation = false;
        asyncLevel.allowSceneActivation = false;

        //_isLoading = true;

        while (asyncMaster.progress < 0.9f && asyncLevel.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeScript.instance.FadeOut());

        asyncMaster.allowSceneActivation = true;
        asyncLevel.allowSceneActivation = true;

        while (!SceneManager.GetSceneByName(levelName).isLoaded)
        {
            yield return null;
        }

        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
    }

    //Use for level editor
    public void StartLoadLevel(string levelName, bool LoadRes = true)
    {
        StartCoroutine(LoadLevel(levelName, LoadRes));
    }

    IEnumerator LoadLevel(string levelName, bool res)
    {
        StaticMapInfo.SetLevelLoadData(levelName, res);
        AsyncOperation asyncMaster = SceneManager.LoadSceneAsync("Master");

        _levelLoadingUI.SetActive(true);
        //_isLoading = true;
        asyncMaster.allowSceneActivation = false;

        while (asyncMaster.progress < 0.9f)
        {
            yield return null;
        }

        asyncMaster.allowSceneActivation = true;
    }

}