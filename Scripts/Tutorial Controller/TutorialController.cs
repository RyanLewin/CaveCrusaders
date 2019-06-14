using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    protected Transform _tutorialUI;
    protected ObjectiveMenuScript _objectiveMenu;
    protected SoundManager _soundManager;

    protected Transform _cameralastPos;

    protected Text _tutorialUIText;
    protected Image _tutorialUIPanel;
    protected CameraScript CameraControl;

    protected WorldController _worldcontoller;
    protected LevelInfo levelInfo;
    void Start()
    {
        _worldcontoller = WorldController.GetWorldController;
        levelInfo = LevelInfo._levelInfo;
        _tutorialUI = levelInfo._tutorialUI;
        _objectiveMenu = levelInfo._objectiveMenu;
        _soundManager = WorldController.GetWorldController._soundManager;
        _tutorialUIText = _tutorialUI.GetComponentInChildren<Text>();
        _tutorialUIPanel = _tutorialUI.GetComponentInChildren<Image>();
        CameraControl = WorldController.GetWorldController.GetComponent<CameraScript>();
        AfterStart();
    }
    public virtual void AfterStart()
    {

    }


    public virtual bool CheckpointReached (Checkpoint checkpoint,string tag)
    {
        return false;
    }

    protected IEnumerator StartStopWaitText(float time)
    {
         if (AudioScript.instance.SubtitlesEnabled)
        {
            _tutorialUIPanel.enabled = true;
            _tutorialUIText.enabled = true;
        }
        yield return new WaitForSeconds(time);
        _tutorialUIPanel.enabled = false;
        _tutorialUIText.enabled = false;
    }

    protected IEnumerator TakeCamraContol (float time)
    {
        yield return new WaitForSeconds(time);
        Camera.main.transform.position = _cameralastPos.position;
        Camera.main.transform.rotation = _cameralastPos.rotation;
    }



}