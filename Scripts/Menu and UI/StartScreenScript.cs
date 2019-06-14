using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    [SerializeField]
    Text _pressKeyText;
    [SerializeField]
    Image _loadingBarFill;
    bool _keyPressed = false;
    bool _isLoading;

    AsyncOperation _currentLoadingOperation;
    bool once = true;
    private void Start()
    {
        _currentLoadingOperation = SceneManager.LoadSceneAsync("MainMenu");
        _currentLoadingOperation.allowSceneActivation = false;
        //_loadingBarFill.fillAmount = 0f;
        _isLoading = true;
        
    }

    void Update()
    {
        if (once)
        {
            Debug.Log("Game started in " + Time.realtimeSinceStartup);
            once = false;
        }
        if (_isLoading)
        {
            //_loadingBarFill.fillAmount = currentLoadingOperation.progress;
            if (_currentLoadingOperation.progress >= 0.9f)
            {
                _isLoading = false;
            }
        }
        else
        {
            if (Input.anyKeyDown && !_keyPressed)
            {
                _keyPressed = true;
                StartCoroutine(GoToMain());
            }
        }
    }

    void FixedUpdate()
    {
        if (!_isLoading)
        {
            Color tempColor = _pressKeyText.color;
            tempColor.a = Mathf.PingPong(Time.time, 1f);
            _pressKeyText.color = tempColor;
        }
    }

    IEnumerator GoToMain()
    {
        yield return StartCoroutine(FadeScript.instance.FadeOut());
        _currentLoadingOperation.allowSceneActivation = true;
    }
}
