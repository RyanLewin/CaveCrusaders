using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Tutorial : TutorialController
{

    [SerializeField] AudioClip _whatsThis10;
    [SerializeField] AudioClip _lava11;
    [SerializeField] AudioClip _nowWereSafeFromLava12;
    [SerializeField] Transform _StartLocation;

    Coroutine _currentCoroutine;

    bool _haveCameraContoles = true;
    bool _lavaWall = false;
    LevelInfo _levelInfo;
    float _cameraWait;

    // Start is called before the first frame update
    public override void AfterStart()
    {
        _levelInfo = LevelInfo._levelInfo;
        //_levelInfo._garageButton.interactable = false;
        //_levelInfo._powerGeneratorButton.interactable = false;
        //_levelInfo._turretButton.interactable = false;
        //_levelInfo._outpostButton.interactable = false;
        //_levelInfo._H2OConverterButton.interactable = false;
        BuildMenuScript.instance._allowBuild["Garage"] = false;
        BuildMenuScript.instance._allowBuild["Outpost"] = false;
        BuildMenuScript.instance._allowBuild["H2OConvertor"] = false;
        BuildMenuScript.instance._allowBuild["Turret"] = false;
        BuildMenuScript.instance._allowBuild["PowerGen"] = false;

        CameraControl.CameraOnRails = true;
        _cameraWait = _lava11.length;
        WorldController.GetWorldController._mushroomSpawn = false;
    }

    // Update is called once per frame
    void Update()
    {

        //_levelInfo._garageButton.interactable = false;
        //_levelInfo._powerGeneratorButton.interactable = false;
        //_levelInfo._turretButton.interactable = false;
        //_levelInfo._outpostButton.interactable = false;
        //_levelInfo._H2OConverterButton.interactable = false;
        

        if (!_lavaWall)
        {
            WorldController _worldController = WorldController.GetWorldController;
            foreach (Building building in _worldController._buildings)
            {
                if (building.tag == "BLOCK" && building._Built)
                {

                    _lavaWall = true;
                    _soundManager.PlaySpeech(_nowWereSafeFromLava12);
                    
                    _tutorialUIText.text = "“Well done Crusader! \nNow that we are safe from the lava, find 10 Energy Crystals";
                    StopCoroutine(_currentCoroutine);
                    _currentCoroutine = StartCoroutine(StartStopWaitText(_nowWereSafeFromLava12.length));
                    StartStopWaitText(_nowWereSafeFromLava12.length);

                }
            }
        }
        if (_haveCameraContoles)
        {
            if (_cameraWait <= 0)
            {
                float step = 100 * Time.deltaTime;
                CameraControl.SetPos(Vector3.MoveTowards(CameraControl.GetPos(), _StartLocation.position, step));
            }
            else
            {
                _cameraWait -= Time.deltaTime;
            }
            float DistanceToTarget = FluidRenderer.FlatDistance(CameraControl.GetPos(), _StartLocation.position);
            if (DistanceToTarget < 1.0f)
            {
                //GIVE CAMRA CONTOLES BACK
                CameraControl.CameraOnRails = false;
                _haveCameraContoles = false;
            }
        }
    }


    public override bool CheckpointReached(Checkpoint checkpoint, string tag)
    {
        switch (checkpoint._checkpointNo)
        {
            case 0:

                CheckPoint0();

                break;
            case 1:
                CheckPoint1();
                break;

        }

        return true;
    }

    void CheckPoint1()
    {
        _soundManager.PlaySpeech(_whatsThis10);

        _tutorialUIText.text = "What’s this Crusader! A monster! \nGive your workers a laser pistol, sound the alarm and shoot that blighter!";
        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(StartStopWaitText(_whatsThis10.length));
        StartStopWaitText(_whatsThis10.length);

    }
    void CheckPoint0()
    {

        _soundManager.PlaySpeech(_lava11);
     
        _tutorialUIText.text = "Lava Crusader! This hot stuff is best avoided. \nUse the Blockades to slow its progress, but make sure you keep them powered";
        _currentCoroutine = StartCoroutine(StartStopWaitText(_lava11.length));
        StartStopWaitText(_lava11.length);
    }

}
