using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    Text _currentStorageCountText, _energyCrystalsCountText, _oreCountText, _notificationText;
    [SerializeField]
    Animator _panicAnim;

    [SerializeField]
    GameObject _pausePanel, _pauseMenu, _tasksPanel, _buildPanel, _objectivesPanel, _minimapPanel, _notificationPanel, _victoryGameButtons, _victoryEditorButtons, _defeatGameButtons, _defeatEditorButtons;
    
    [SerializeField]
    Builder Builder;
    MenuScript MenuScript;

    GameObject _victoryPanel, _defeatPanel, _statsPanel;
    Text _defeatText;

    void Start()
    {
        MenuScript = GetComponent<MenuScript>();
        _panicAnim.gameObject.SetActive(WorldController.GetWorldController._AlertMode);

        _victoryPanel = LevelInfo._levelInfo._victoryPanal;
        _defeatPanel = LevelInfo._levelInfo._defeatPanal;
        _statsPanel = LevelInfo._levelInfo._statsDisplay;
        _defeatText = LevelInfo._levelInfo._defeatText;
    }

    void Update()
    {
        UpdateResources();
        if (WorldController.GetMode() == WorldController.CurrentGameMode.Play)
        {
            BuildMenuScript.instance.UpdateBuildMenu();

            if (ControlScript.instance.GetControl("Pause").InputDown)
            {
                if (!_statsPanel.activeInHierarchy)
                {
                    PausePanelVisible();

                    if (MenuScript.GamePaused)
                    {
                        MenuScript.ResumeGame();
                    }
                    else
                    {
                        MenuScript.PauseGame();
                    }
                }
            }
            if (ControlScript.instance.GetControl("Panic").InputDown && !MenuScript.GamePaused)
            {
                Panic();
            }
            if (ControlScript.instance.GetControl("Build Menu").InputDown && !MenuScript.GamePaused)
            {
                BuildListVisible();
            }
            if (ControlScript.instance.GetControl("Tasks Menu").InputDown && !MenuScript.GamePaused)
            {
                TaskListVisible();
            }
            if (ControlScript.instance.GetControl("Objectives Menu").InputDown && !MenuScript.GamePaused)
            {
                ObjectivesListVisible();
            }
            if (ControlScript.instance.GetControl("Minimap").InputDown && !MenuScript.GamePaused)
            {
                MinimapPanelVisible();
            }
        }
    }

    /// <summary>
    /// Call builder set build index
    /// </summary>
    /// <param name="index">Building type index</param>
    public void SetBuildIndex(int index)
    {
        Builder.UISetBuild(index);
    }    
    
    /// <summary>
    /// Call builder cancel
    /// </summary>
    public void CancelBuild()
    {
        Builder.CancelBuild();
    }

    public void ShowNotification(string message)
    {
        _notificationText.text = message;
        _notificationPanel.SetActive(true);
        StartCoroutine(WaitHideNotification());
    }

    IEnumerator WaitHideNotification()
    {
        yield return new WaitForSeconds(5f);
        _notificationPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the UI to display the current ore and energy crystal counts
    /// </summary>
    public void UpdateResources()
    {
        //if (_energyCrystalsCountText.text != WorldController.GetWorldController._energyCrystalsCount.ToString() || _oreCountText.text != WorldController.GetWorldController._oreCount.ToString())
        //{
            _energyCrystalsCountText.text = WorldController.GetWorldController._energyCrystalsCount.ToString();
            _oreCountText.text = WorldController.GetWorldController._oreCount.ToString();
        
            int currentCount = WorldController.GetWorldController._energyCrystalsCount + WorldController.GetWorldController._oreCount;
            _currentStorageCountText.text = currentCount.ToString() + " / " + WorldController.GetWorldController._maxStorage.ToString();

            if (WorldController.GetWorldController.CheckStorage())
            {
                _currentStorageCountText.color = Color.red;
            }
            else
            {
                _currentStorageCountText.color = Color.white;
            }

        //}
    }

    /// <summary>
    /// Set panic panel anim state to play and stop panic animation
    /// </summary>
    public void Panic()
    {
        WorldController.GetWorldController._AlertMode = !WorldController.GetWorldController._AlertMode;
        WorldController.GetWorldController._soundManager.BattleMode = WorldController.GetWorldController._AlertMode;
        if (WorldController.GetWorldController._AlertMode)
        {
            WorldController.GetWorldController._soundManager.SoundAlarm();
        }
      _panicAnim.gameObject.SetActive(WorldController.GetWorldController._AlertMode);
      _panicAnim.SetBool("Panic", !_panicAnim.GetBool("Panic"));
        
    }

    /// <summary>
    /// Set task list visiblity
    /// </summary>
    public void TaskListVisible()
    {
        _tasksPanel.SetActive(!_tasksPanel.activeInHierarchy);
        //_buildPanel.SetActive(false);
    }

    /// <summary>
    /// Set build list visiblity
    /// </summary>
    public void BuildListVisible()
    {
        BuildMenuScript.instance.ClearToolTip();
        _buildPanel.SetActive(!_buildPanel.activeInHierarchy);
        //_tasksPanel.SetActive(false);
    }

    /// <summary>
    /// Set objective list visiblity
    /// </summary>
    public void ObjectivesListVisible()
    {
        _objectivesPanel.SetActive(!_objectivesPanel.activeInHierarchy);
        ObjectiveMenuScript.instance.ChangeButtonText((_objectivesPanel.activeInHierarchy == true) ? "Hide Objectives" : "Show Objectives");
    }

    /// <summary>
    /// Set minimap panel visiblity
    /// </summary>
    public void MinimapPanelVisible()
    {
        _minimapPanel.GetComponentInChildren<MiniMap>().EnlargeMiniMap();
        //_minimapPanel.SetActive(!_minimapPanel.activeInHierarchy);
    }

    /// <summary>
    /// Set pause panel visiblity
    /// </summary>
    public void PausePanelVisible()
    {
        _pausePanel.SetActive(!_pausePanel.activeInHierarchy);
        _pausePanel.transform.GetChild(1).gameObject.SetActive(false);

        SetPauseMenu();
    }

    public void SetPauseMenu()
    {
        if (WorldController.GetMode() == WorldController.CurrentGameMode.Play)
        {
            _pauseMenu.transform.GetChild(0).gameObject.SetActive(true);
            _pauseMenu.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            _pauseMenu.transform.GetChild(0).gameObject.SetActive(false);
            _pauseMenu.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Show victory panel
    /// </summary>
    public void CompletedLevel()
    {
        MenuScript.PauseGame();
        if (WorldController.GetMode() == WorldController.CurrentGameMode.Play)
        {
            _victoryGameButtons.SetActive(true);
            _victoryEditorButtons.SetActive(false);
        }
        else
        {
            _victoryGameButtons.SetActive(false);
            _victoryEditorButtons.SetActive(true);
        }
        _victoryPanel.SetActive(true);
        _statsPanel.SetActive(true);
    }

    /// <summary>
    /// Show defeat panel
    /// </summary>
    public void FailedLevel(string defeatReason)
    {
        MenuScript.PauseGame();
        if (WorldController.GetMode() == WorldController.CurrentGameMode.Play)
        {
            _defeatGameButtons.SetActive(true);
            _defeatEditorButtons.SetActive(false);
        }
        else
        {
            _defeatGameButtons.SetActive(false);
            _defeatEditorButtons.SetActive(true);
        }
        _defeatPanel.SetActive(true);
        _statsPanel.SetActive(true);
        if (_defeatText != null)
        {
            _defeatText.text = defeatReason;
        }
    }
}