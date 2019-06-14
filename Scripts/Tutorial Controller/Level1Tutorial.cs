using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level1Tutorial : TutorialController
{

    Objective _tutorialObjective;
    Objective _tutorialObjective2;

    [SerializeField] GameObject _arrow;
    [SerializeField] Level1Objectives _level1Objective;
    [SerializeField] Button _oxengenGenButton, _changeToolButton;
    GameObject _tutorialArrow;

    Coroutine _currentCoroutine;

    WorldController _worldController;

    HQ _hq;
    bool _buildOxegenGen = false;
    bool _hqAccesible = false;
    bool _doOnce = true;
    bool _checkpoint3 = false;
    bool _checkpoint4 = false;
    bool _checkpoint4Part2 = false;
    bool _checkpoint4Part3 = false;

    bool chaa = true;
    //Sounds
    
    [SerializeField] AudioClip _helloCrusader1;
    [SerializeField] AudioClip _seeThatCrusader2;
    [SerializeField] AudioClip _ahTheHQBuilding3;
    [SerializeField] AudioClip _ourCurrentMining4;
    [SerializeField] AudioClip _wellDone5;
    [SerializeField] AudioClip _mineThoughThatWall6;
    [SerializeField] AudioClip _ohhNoCrusader7;
    [SerializeField] AudioClip _selectAWorker8;
    [SerializeField] AudioClip _generatorWillProvide9;

    LevelInfo _levelInfo;

    public override void AfterStart()
    {
        _levelInfo = LevelInfo._levelInfo;
        //_oxengenGenButton = _levelInfo._oxengenGenButton;
        _changeToolButton = _levelInfo._changeToolButton;
        //_levelInfo._lavaBlockadeButton.interactable = false;
        //_levelInfo._garageButton.interactable = false;
        //_levelInfo._powerGeneratorButton.interactable = false;
        //_levelInfo._turretButton.interactable = false;
        //_levelInfo._outpostButton.interactable = false;
        //_levelInfo._H2OConverterButton.interactable = false;
        BuildMenuScript.instance._allowBuild["OxyGen"] = false;
        BuildMenuScript.instance._allowBuild["Blockade"] = false;
        BuildMenuScript.instance._allowBuild["Garage"] = false;
        BuildMenuScript.instance._allowBuild["Outpost"] = false;
        BuildMenuScript.instance._allowBuild["H2OConvertor"] = false;
        BuildMenuScript.instance._allowBuild["Turret"] = false;
        BuildMenuScript.instance._allowBuild["PowerGen"] = false;


        _worldController = WorldController.GetWorldController;

        _worldController._UseO2 = false;
        _worldController._landSlides = false;
        _worldController._mushroomSpawn = false;

        _hq = _worldController._HQ;
        _hq.FogRange.SetActive(false);

        FogOfWarDecalController DecalCon = FindObjectOfType<FogOfWarDecalController>();
        if (DecalCon != null)
        {
            
                DecalCon.Clear = true;
            
        }
    }



    public override bool CheckpointReached (Checkpoint checkpoint,string tag)
    {
        if(_tutorialUI == null)
        {
            return false;
        }
        switch(checkpoint._checkpointNo)
        {
            case 0:
                
                 CheckPoint0();
             
                 break;
            case 1:
                if (tag == "Ore")
                {
                    CheckPoint1();
                    
                }
                else
                {
                    return false;
                }
                break;
            case 2:
                CheckPoint2();
                break;
            case 3:
                if (tag == "EnergyCrystal")
                {
                    CheckPoint3();
                }
                else
                {
                    return false;
                }
                break;
            case 4:
                CheckPoint4();
                break;
 
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        //_levelInfo._lavaBlockadeButton.interactable = false;
        //_levelInfo._garageButton.interactable = false;
        //_levelInfo._powerGeneratorButton.interactable = false;
        //_levelInfo._turretButton.interactable = false;
        //_levelInfo._outpostButton.interactable = false;
        //_levelInfo._H2OConverterButton.interactable = false;


        if (_doOnce)
        {
            _worldController = WorldController._worldController;

            _worldController._UseO2 = false;

            _hq = _worldController._HQ;
            _hq.FogRange.SetActive(false);

            _doOnce = false;
        }
        if (chaa && _worldController._workers.Count == 5)
        {
            foreach (Unit unit in _worldController._workers)
            {
                unit._canPickup = false;
                unit._useO2 = false;
            }
            chaa = false;
        }

        if(!_buildOxegenGen)
        {
            //_oxengenGenButton.interactable = false;
            BuildMenuScript.instance._allowBuild["OxyGen"] = false;
        }

        if (!_hqAccesible)
        {
            _changeToolButton.interactable = false;
            _hq._barsUpdater.gameObject.SetActive(false);
        }

        if (_checkpoint3&&(int)_worldController._miningLevel > 0)
        {
            Object.Destroy(_tutorialArrow);
            _tutorialArrow = Instantiate(_arrow, new Vector3(32.670f, 13f, 55.6f), new Quaternion(0, 0, 0, 0));
            _soundManager.PlaySpeech(_mineThoughThatWall6);
            
            _tutorialUIText.text = "Right Crusader, mine through that wall and lets find more Energy Crystals!";
            StopCoroutine(_currentCoroutine);
            _currentCoroutine =StartCoroutine(StartStopWaitText(_mineThoughThatWall6.length));
            StartStopWaitText(_mineThoughThatWall6.length);

            _tutorialObjective.UpdateProgress(1);
            _tutorialObjective = new Objective("Mine through Rock", true, 1);
            _objectiveMenu.ObjectivesList.Add(_tutorialObjective);
            _objectiveMenu.UpdateObjectiveList();
            _checkpoint3 = false;
        }
        if(_checkpoint4)
        {
            foreach(Building building in _worldController._buildings)
            {
               if(building.tag == "GEN")
                {

                    _tutorialObjective2 = new Objective("Get a hammer.Right click a worker to change tool", true, 1);
                    _objectiveMenu.ObjectivesList.Add(_tutorialObjective2);
                    _objectiveMenu.UpdateObjectiveList();

                    _soundManager.PlaySpeech(_selectAWorker8);
               
                    _tutorialUIText.text = "Select a worker and send them to the HQ. \nThey will need a hammer to make the Generator";
                    _checkpoint4 = false;
                    _checkpoint4Part2 = true;
                    _checkpoint4Part3 = true;
                    StopCoroutine(_currentCoroutine);
                    _currentCoroutine = StartCoroutine(StartStopWaitText(_selectAWorker8.length));
                    StartStopWaitText(_selectAWorker8.length);
                   
                }

            }
        }

        if(_checkpoint4Part2)
        {
            foreach(Unit unit in _worldController._workers)
            {
                Worker worker = unit.GetComponent<Worker>();
                if (worker != null)
                {
                    if (worker._currentTool == Unit.UnitTool.Hammer)
                    {
                        _tutorialObjective2.UpdateProgress(1);
                        _checkpoint4Part2 = false;
                    }
                }
            }
        }
        if (_checkpoint4Part3)
        {

            foreach (Building building in _worldController._buildings)
            {
                if (building.tag == "GEN")
                {
                    if (building._Built)
                    {
                        _checkpoint4Part3 = false;

                        _soundManager.PlaySpeech(_generatorWillProvide9);
                        _tutorialObjective.UpdateProgress(1);

                        _tutorialUIText.text = "Well done Crusader! The generator will provide your workers with a steady stream of oxygen, but make sure you keep it fueled on energy crystals!";
                        _level1Objective.addObjectives();
                        StopCoroutine(_currentCoroutine);
                        _currentCoroutine = StartCoroutine(StartStopWaitText(_generatorWillProvide9.length));
                        StartStopWaitText(_generatorWillProvide9.length);

                    }
                }

            }
        }


        if (!AudioScript.instance.SubtitlesEnabled)
        {
            _tutorialUIText.enabled = false;
        }

    }

    void CheckPoint0()
    {

        _tutorialArrow = Instantiate(_arrow, new Vector3(40.08f, 13f, 110.46f), new Quaternion(0, 0, 0, 0));
        _soundManager.PlaySpeech(_helloCrusader1);
        _tutorialUIText.text = "Hello Crusader. It appears your workers have over shot. \nSelect a worker and order them to mine that wall. You need to get to the HQ in the next cavern! \nRemember Crusader you can pan and angle the camera!";
        _tutorialObjective = new Objective("Mine to the next cavern. Right click a wall and select mine", true, 1);
        _objectiveMenu.ObjectivesList.Add(_tutorialObjective);
        _objectiveMenu.UpdateObjectiveList();
   
        _currentCoroutine = StartCoroutine(StartStopWaitText(_helloCrusader1.length));
        StartStopWaitText(_helloCrusader1.length);
    }

    void CheckPoint1()
    {
        Object.Destroy(_tutorialArrow);
        _soundManager.PlaySpeech(_seeThatCrusader2);

        _tutorialUIText.text = "See that Crusader, those rocks drop Ore. \nYou will need that later for building";
        StartCoroutine(StartStopWaitText(_seeThatCrusader2.length));
        StartStopWaitText(_seeThatCrusader2.length);
    }

    void CheckPoint2()
    {
        _hq.FogRange.SetActive(true);
        _soundManager.PlaySpeech(_ahTheHQBuilding3);
       
        _tutorialUIText.text = "Ah the HQ building! \nThis building is the heart of your operation. With it you can produce more Crusaders and store a small amount of resources. \nSee how your crusaders are collecting that ore for you? Unless you tell them what to do workers will automatically pick up tasks. Open up the task list and have a look!";

        _tutorialObjective.UpdateProgress(1);
        _hqAccesible = true;
        _changeToolButton.interactable = true;
        _hq._barsUpdater.gameObject.SetActive(true);
        

        foreach (Unit unit in _worldController._workers)
        {
            unit._canPickup = true;
        }
        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(StartStopWaitText(_ahTheHQBuilding3.length));
        StartStopWaitText(_ahTheHQBuilding3.length);
        StartCoroutine(CheckPoint2Part2());
        CheckPoint2Part2();

    }


    IEnumerator CheckPoint2Part2()
    {

        yield return new WaitForSeconds(_ahTheHQBuilding3.length + 1);
        _soundManager.PlaySpeech(_ourCurrentMining4);
        
        _tutorialUIText.text = "Well Crusader it appears our current mining equipment can’t get through that rock. \nWe must find an energy crystal and upgrade the HQ! \nI am sure a skilled fellow like yourself can find one";

       _tutorialArrow = Instantiate(_arrow, new Vector3(32.670f, 13f, 55.6f), new Quaternion(0,0,0,0));

        _tutorialObjective = new Objective("Find an Energy Crystal", true, 1);
        _objectiveMenu.ObjectivesList.Add(_tutorialObjective);
        _objectiveMenu.UpdateObjectiveList();

        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(StartStopWaitText(_ourCurrentMining4.length));
        StartStopWaitText(_ourCurrentMining4.length);
    }

    void CheckPoint3()
    {
        _tutorialObjective.UpdateProgress(1);
        _tutorialObjective = new Objective("Upgrade HQ. Right click the HQ", true, 1);
        _objectiveMenu.ObjectivesList.Add(_tutorialObjective);
        _objectiveMenu.UpdateObjectiveList();
        Object.Destroy(_tutorialArrow);
        _tutorialArrow = Instantiate(_arrow, new Vector3(44.54f, 13f, 79.74f), new Quaternion(0, 0, 0, 0));

        _soundManager.PlaySpeech(_wellDone5);
        ;
        _tutorialUIText.text = "Well done Crusader, you found an Energy Crystal! \nTake that back to the HQ and upgrade it!";
        _checkpoint3 = true;

        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(StartStopWaitText(_wellDone5.length));
        StartStopWaitText(_wellDone5.length);
    }

    void CheckPoint4()
    {
        _tutorialObjective.UpdateProgress(1);

        _tutorialObjective = new Objective("Build an Oxygen Generator", true, 1);
        _objectiveMenu.ObjectivesList.Add(_tutorialObjective);
        _objectiveMenu.UpdateObjectiveList();

        Object.Destroy(_tutorialArrow);
        _worldController._UseO2 = true;
        foreach (Unit unit in _worldController._workers)
        {
            unit._useO2 = true;
        }
        _soundManager.PlaySpeech(_ohhNoCrusader7);
       
        _tutorialUIText.text = "Oh no Crusader! \nIt appears this cavern's oxygen supply is running out! \nUse the building menu to construct an Oxygen Generator! You don’t want to be caught short of breath!";
      
        _checkpoint4 = true;
        _buildOxegenGen = true;
        BuildMenuScript.instance._allowBuild["OxyGen"] = true;
        StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(StartStopWaitText(_ohhNoCrusader7.length));
        StartStopWaitText(_ohhNoCrusader7.length);

    }

}
