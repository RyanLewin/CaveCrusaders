using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldController : MonoBehaviour
{
    public static WorldController _worldController;
    public UIScript UIScript;
    public TaskLibrary TaskLib;
    public List<Building> _buildings = new List<Building>();
    public HQ _HQ;
    public List<Unit> _workers = new List<Unit>();
    public int _workerLimit = 15;
    public List<EnergyCrystal> _energyCrystals = new List<EnergyCrystal>();
    public List<Monster> _monsters = new List<Monster>();
    public List<Tile> _defultTile = new List<Tile>();

    public GameObject _fogOfWar;
    public GameObject mouseSelection;
    public bool _landSlides = true;
    [SerializeField] GameObject _mushroomCluster;
    public bool _mushroomSpawn = true;
    public List<MushroomCluster> _mushroomClusters = new List<MushroomCluster>();
    public List<RockScript> _landslideRocks = new List<RockScript>();
    public MouseSelectionController MouseSelectionControl = null;
    public LevelStatsController _levelStatsController;
    public SoundManager _soundManager;
    public float GrowSpeed = 0.02f;

    public int _energyCrystalsCount { get; set; }
    int _ore;
    public int _oreCount
    {
        get => _ore;
        set
        {
            _ore = value;
            FillSkips();
            AnimateSkips(CheckStorage());
        }
    }
    float previousFillLevel;
    public int _maxStorage { get; set; }
    public bool _AlertMode { get; set; }
    public bool _UseO2 { get; set; } = true;
    public int _energyCrystalsOnMap { get; set; }



    public enum MiningLevel { one, two, three };
    public MiningLevel _miningLevel;
    //Debug Options
    public bool DEBUG_INSTANT_MINE = false;
    public bool DEBUG_FAST_WORKER_MOVE = false;
    public bool DEBUG_FAST_GROW = false;
    public bool DEBUGSPAWNGARY = false;
    public bool UseNewMouseSelect = false;
    public Camera MainCam = null;
    public CPU_FOW CPUFOW = null;
    public enum CurrentGameMode { Play, LiveEdit, Editor, EditorPlay }
    public TileMap3D TileMap = null;
    [SerializeField]
    GameObject GameUI;
    LevelEditor Editor;
    [ReadOnly, SerializeField]
    CurrentGameMode CurrentPlayMode = CurrentGameMode.Play;
    float _spawnMushRoomCluster = 2;
    float _randomLandslideTime;
    private void Start()
    {
        _randomLandslideTime = Random.Range(60, 180);
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
        _worldController = this;
        MouseSelectionControl = GetComponent<MouseSelectionController>();
        TaskLib = GetComponent<TaskLibrary>();
        TileMap = GetComponent<TileMap3D>();
        if (UseNewMouseSelect)
        {
            if (mouseSelection != null)
            {
                mouseSelection.GetComponent<MouseSelection>().enabled = false;
            }
        }
        else
        {
            MouseSelectionControl.enabled = false;
        }
        MainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        CPUFOW = GetComponentInChildren<CPU_FOW>();
        Editor = FindObjectOfType<LevelEditor>();
        if (StaticMapInfo.LoadingIntoLevelEditor)
        {
            SetGamePlayMode(StaticMapInfo.LevelEditorMode);
        }
        UpdateMode();
        FindOrCreateObjectsController();
    }
    [SerializeField]
    GameObject DefaultObjectiveController;
    void FindOrCreateObjectsController()
    {
        ObjectivesContoler C = FindObjectOfType<ObjectivesContoler>();
        if (C == null && !StaticMapInfo.IsLevelInBuilt && IsPlaying())
        {
            Debug.Log("This level is from the level editor right?");
            Instantiate(DefaultObjectiveController);
        }
    }
    public static bool IsRunning()
    {
        return GetWorldController.GetComponent<TileMap3D>().IsSimulating();
    }

    public static void SetFowState(bool state, bool clear = false)
    {
        FogOfWarDecalController FOW = WorldController.GetWorldController._fogOfWar.GetComponent<FogOfWarDecalController>();
        if (FOW == null)
        {
            return;
        }
        FOW.EnableCloseFog = state;
        FOW.Clear = clear;
        FOW.UpdateCamSizes();
    }

    public void SetGamePlayMode(CurrentGameMode state)
    {
        if (state == CurrentPlayMode)
        {
            return;
        }
        if (state == CurrentGameMode.Play || state == CurrentGameMode.EditorPlay)
        {
            OnPlayStart();
        }
        else
        {
            OnPlayEnd();
        }
        CurrentPlayMode = state;
        Debug.Log("Play mode changed to " + state.ToString());
        UpdateMode();
    }

    public static bool IsPlaying()
    {
        return (GetWorldController.CurrentPlayMode == CurrentGameMode.Play || GetWorldController.CurrentPlayMode == CurrentGameMode.EditorPlay);
    }

    public static CurrentGameMode GetMode()
    {
        return GetWorldController.CurrentPlayMode;
    }

    void UpdateMode()
    {
        if (GameUI != null)
        {
            GameUI.SetActive(IsPlaying());
        }
        Editor.UpdateMode(CurrentPlayMode);
        TileMap.SetSimulationState(IsPlaying());
        TileMap.SetDirty();//just push an update to be safe
        SetFowState(IsPlaying());
    }

    public void Update()
    {
        _soundManager.BattleMode = _AlertMode;
        if (_landSlides && IsPlaying())
        {
            _randomLandslideTime -= Time.deltaTime;
            if (_randomLandslideTime <= 0)
            {
                _randomLandslideTime = Random.Range(60, 180);
                if (_landslideRocks.Count > 0)
                {
                    RockScript thisRock = _landslideRocks[Random.Range(0, _landslideRocks.Count)];
                    if (thisRock == null)
                    {
                        _landslideRocks.Remove(thisRock);
                    }
                    else
                    {
                        thisRock.LandSlide(false);
                    }

                }
            }
        }

        if (_mushroomSpawn && IsPlaying())
        {
            _spawnMushRoomCluster -= Time.deltaTime;
            if (_spawnMushRoomCluster <= 0)
            {
                _spawnMushRoomCluster = Random.Range(60, 180);

                if (_mushroomClusters.Count <= 5)
                {

                    Tile chosenTile;
                    do
                    {
                        int spawnTile = Random.Range(0, _defultTile.Count);
                        chosenTile = _defultTile[spawnTile];
                    }
                    while (chosenTile.GetComponent<Tile>()._CurrentType != Fluid.FluidType.None || chosenTile.GetComponentInChildren<MushroomCluster>() != null);


                    Vector3 newLocation = chosenTile.transform.position;
                    GameObject newCluster = Instantiate(_mushroomCluster, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));
                
                    if (newCluster != null)
                    {
                        newCluster.GetComponent<MushroomCluster>().FirstMushRoom(newLocation.x, newLocation.y, newLocation.z);
                    }
                }

            }
        }
    }

    public void OnPlayStart()
    {
        SetFowState(true, true);
        TileMap.BroadcastEditorPlay();

    }

    public void OnPlayEnd()
    {

    }
    /// <summary>
    /// Checks if the amount of energy crystals and the amount of ore are at or exceeding the max storage.
    /// </summary>
    /// <returns> Returns true for full/overflow, false for not full. </returns>
    public bool CheckStorage()
    {
        if (_energyCrystalsCount + _oreCount >= _maxStorage)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FillSkips()
    {
        float fillPercentage = (float)(_energyCrystalsCount + _oreCount) / (float)_maxStorage;
        fillPercentage *= 100;

        int fillLevel = 0;
        if (fillPercentage > 0 && fillPercentage <= 30)
        {
            fillLevel = 1;
        }
        else if (fillPercentage > 30 && fillPercentage <= 60)
        {
            fillLevel = 2;
        }
        else if (fillPercentage > 60)
        {
            fillLevel = 3;
        }

        if (fillLevel == previousFillLevel)
            return;

        foreach (var building in _buildings)
        {
            if (building.GetID() == (int)Tile.TileTypeID.Skip)
            {
                List<GameObject> fillTiles = building.GetComponent<Skip>()._fillTiles;
                foreach (var fillTile in fillTiles)
                {
                    fillTile.SetActive(false);
                }
                switch (fillLevel)
                {
                    case 1:
                        fillTiles[0].SetActive(true);
                        break;
                    case 2:
                        fillTiles[1].SetActive(true);
                        break;
                    case 3:
                        fillTiles[2].SetActive(true);
                        break;
                }
                previousFillLevel = fillLevel;
            }
        }
    }

    public void AnimateSkips(bool closed)
    {
        foreach (var building in _buildings)
        {
            if (building.GetID() == (int)Tile.TileTypeID.Skip)
            {
                if (building._Built)
                {
                    building.GetComponentInChildren<Animator>().SetBool("Closed", closed);
                }
            }
        }
    }

    public static WorldController GetWorldController { get { return _worldController; } }
}
