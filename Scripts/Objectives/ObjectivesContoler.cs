using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesContoler : MonoBehaviour
{

    [SerializeField] Transform _cameraStartLocation;

    protected ObjectiveMenuScript _objectiveMenu;
    //GameObject _victoryPanal;
    //GameObject _defeatPanal;
    //GameObject _statsDisplay;
    //Text _defeatText;

    bool _winLoss = false;

    protected string _defeatReason = "Someone Programmed This Incorrectly";

    protected WorldController _worldController;
    // Start is called before the first frame update
    void Start()
    {
        _worldController = WorldController.GetWorldController;
        if (_cameraStartLocation != null)
        {
            _worldController.GetComponent<CameraScript>().SetCameraPosAndRot(_cameraStartLocation.position, _cameraStartLocation.rotation.eulerAngles.y);
            _worldController.GetComponent<CameraScript>().SetZoom(0.5f);
        }
        else
        {
            HQ HQPos = WorldController.GetWorldController._HQ;
            if (HQPos != null)
            {
                _worldController.GetComponent<CameraScript>().SetCameraPosAndRot(HQPos.transform.position, 0.0f);
            }
            _worldController.GetComponent<CameraScript>().SetZoom(0.5f);
        }
        LevelInfo levelInfo = LevelInfo._levelInfo;
        
        _objectiveMenu = levelInfo._objectiveMenu;
        //_victoryPanal = levelInfo._victoryPanal;
        //_defeatPanal = levelInfo._defeatPanal;
        //_statsDisplay = levelInfo._statsDisplay;
        //_defeatText = levelInfo._defeatText;
        AfterStart();
    }

    protected virtual void AfterStart()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (!_winLoss)
        {
            if (WinCondtions())
            {
                _worldController._levelStatsController.GameEnd();
                _worldController.UIScript.CompletedLevel();
                //_victoryPanal.SetActive(true);
                //_statsDisplay.SetActive(true);
                _worldController._soundManager.WinLossMusic(true);
                _winLoss = true;
            }
            else if (LoseCondtions())
            {
                _worldController._levelStatsController.GameEnd();
                _worldController.UIScript.FailedLevel(_defeatReason);
                //_defeatPanal.SetActive(true);
                //_statsDisplay.SetActive(true);
                _worldController._soundManager.WinLossMusic(false);
                //if (_defeatText != null)
                //{
                //    _defeatText.text = _defeatReason;
                //}
                _winLoss = true;
            }
        }
        WithUpdate();
    }

    protected virtual void WithUpdate()
    {

    }

    protected bool StanderLossCondtions()
    {
        if (_worldController._HQ == null)
        {
            _defeatReason = "HQ was destroyed";
            return true;
        }
        if (_worldController._workers.Count == 0 && _worldController._oreCount < _worldController._HQ.workerCost)
        {
            _defeatReason = "You ran out of Ore and have no workers";
            return true;
        }
        return false;
    }

    protected virtual bool LoseCondtions()
    {
        if (StanderLossCondtions())
        {
            return true;
        }
        return false;

    }
    protected virtual bool WinCondtions()
    {
        return false;
    }

}
