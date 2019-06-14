using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScript : MonoBehaviour
{
    [SerializeField]
    MenuScript _menuScript;
    bool _keyPressed = false;

    AsyncOperation _currentLoadingOperation;

    void Start()
    {
        _currentLoadingOperation = SceneManager.LoadSceneAsync("MainMenu");
        _currentLoadingOperation.allowSceneActivation = false;
    }

    void Update()
    {
        if (Input.anyKeyDown && !_keyPressed)
        {
            _keyPressed = true;
            Finish();
        }
    }

    public void Finish()
    {
        FadeScript.instance.FadeOut();
        _currentLoadingOperation.allowSceneActivation = true;
    }
}
