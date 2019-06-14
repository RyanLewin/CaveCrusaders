using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUIScript : MonoBehaviour
{
    public static LevelEditorUIScript instance;

    [SerializeField]
    InputField _saveMapNameInput, _newMapSizeXInput, _newMapSizeYInput;
    [SerializeField]
    Dropdown _mapsDropdown, _tileFluidTypeDropDown, _buildingLevelDropdown;
    [SerializeField]
    Text _tileTypeText, _playBoldText, _editBoldText, _descriptionText;
    [SerializeField]
    Slider _tileFluidLevelSlider;
    [SerializeField]
    Toggle _rockCrystalToggle;
    [SerializeField]
    Button _playControlPlay, _playControlEdit;

    [SerializeField]
    GameObject _pausePanel, _buildPanel, _dialogPanel, _savePanel, _deletePanel, _newPanel, _yourMapsPanel, _pauseMenu, _mapPanel, _tilePanel, _mapsContentView, _mapContentItemPrefab, _tileContentView, _playControlDisablePanel, _yourMapsButton;

    [SerializeField]
    MapSaveController MapSaver;
    [SerializeField]
    LevelEditor LevelEditor;
    [SerializeField]
    TileMap3D TileMap;
    [SerializeField]
    TileLibrary TileLib;

    MenuScript MenuScript;

    bool doOnce = true;

    List<string> _tileDescriptions = new List<string>()
    {
        "DEFAULT",
        "LAVA",
        "WATER",
        "DIRT",
        "LOOSE ROCK",
        "HARD ROCK",
        "SOLID ROCK",
        "1 WORKER",
        "2 WORKERS",
        "HQ",
        "SKIP",
        "OXY-GEN",
        "LAVA BLOCKADE",
        "GARAGE",
        "OUTPOST",
        "H2O CONVERTOR",
        "TURRET",
        "POWER-GEN",
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        TileMap = WorldController.GetWorldController.TileMap;
        TileLib = WorldController.GetWorldController.GetComponentInChildren<TileLibrary>();
        MapSaver = WorldController.GetWorldController.GetComponentInChildren<MapSaveController>();
        MenuScript = GetComponent<MenuScript>();
        if (StaticMapInfo.Level != "" && StaticMapInfo.LoadingIntoLevelEditor)
        {
            HideAllDialogPanels();
            _yourMapsPanel.SetActive(false);
          //  MapSaver.LoadTileSet(StaticMapInfo.Level);
        }
    }

    void Update()
    {
        if (WorldController.GetMode() != WorldController.CurrentGameMode.Play)
        {
            UpdatePlayControl();
            _yourMapsButton.SetActive(WorldController.GetMode() == WorldController.CurrentGameMode.Editor);

            if (doOnce)
            {
                PopulateMapDropdown();
                doOnce = false;
            }
            if (_tilePanel.activeInHierarchy)
            {
                if (LevelEditor.CurrentSelectedTile != null)
                {
                    _tileFluidLevelSlider.value = LevelEditor.CurrentSelectedTile.GetFluidLevel();
                }
                else
                {
                    _tilePanel.SetActive(false);
                }
            }

            if (ControlScript.instance.GetControl("Pause").InputDown)
            {
                if (!LevelInfo._levelInfo._statsDisplay.activeInHierarchy)
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
            if (ControlScript.instance.GetControl("Build Menu").InputDown)
            {
                BuildListVisible();
            }
            if (ControlScript.instance.GetControl("Grid Menu").InputDown)
            {
                MapPanelVisible();
            }

            if (WorldController.GetMode() == WorldController.CurrentGameMode.Editor)
            {
                _editBoldText.gameObject.SetActive(true);
                _playBoldText.gameObject.SetActive(false);
            }
            else if (WorldController.GetMode() == WorldController.CurrentGameMode.EditorPlay)
            {
                _editBoldText.gameObject.SetActive(false);
                _playBoldText.gameObject.SetActive(true);
            }
        }
    }

    void UpdatePlayControl()
    {
        _playControlDisablePanel.SetActive(_yourMapsPanel.activeInHierarchy || _dialogPanel.activeInHierarchy || LevelInfo._levelInfo._statsDisplay.activeInHierarchy);
        _playControlEdit.interactable = !_yourMapsPanel.activeInHierarchy && !_dialogPanel.activeInHierarchy && !LevelInfo._levelInfo._statsDisplay.activeInHierarchy;
        _playControlPlay.interactable = !_yourMapsPanel.activeInHierarchy && !_dialogPanel.activeInHierarchy && !LevelInfo._levelInfo._statsDisplay.activeInHierarchy;
    }

    /// <summary>
    /// Set build list visiblity
    /// </summary>
    public void BuildListVisible()
    {
        ClearToolTip();
        _buildPanel.SetActive(!_buildPanel.activeInHierarchy);
        //_mapPanel.SetActive(false);
    }

    /// <summary>
    /// Set map panel visiblity
    /// </summary>
    public void MapPanelVisible()
    {
        _mapPanel.SetActive(!_mapPanel.activeInHierarchy);
        //_buildPanel.SetActive(false);

        //_currentMapSizeXInput.text = TileMap.GridXSize_.ToString();
        //_currentMapSizeYInput.text = TileMap.GridYSize_.ToString();
    }

    /// <summary>
    /// Set tile panel visiblity and set property values
    /// </summary>
    public void TilePanelVisible()
    {
        Tile selectedTile = LevelEditor.CurrentSelectedTile;

        string tileType = selectedTile.GetType().ToString();
        if (selectedTile is RockScript)
        {
            tileType = selectedTile.GetComponent<RockScript>().RockType.ToString();
            _rockCrystalToggle.transform.parent.gameObject.SetActive(true);
            _buildingLevelDropdown.transform.parent.gameObject.SetActive(false);

            int rockTypeId = selectedTile.GetComponent<RockScript>().GetID();
            _rockCrystalToggle.isOn = (rockTypeId == (int)Tile.TileTypeID.DirtEnergy || rockTypeId == (int)Tile.TileTypeID.HardRockEnergy || rockTypeId == (int)Tile.TileTypeID.LooseRockEnergy);
        }
        else if (selectedTile is Building)
        {
            //_buildingLevelDropdown.transform.parent.gameObject.SetActive(true);
            _rockCrystalToggle.transform.parent.gameObject.SetActive(false);
            _buildingLevelDropdown.value = selectedTile.GetComponent<Building>().BuildingLevel;
        }
        else
        {
            _rockCrystalToggle.transform.parent.gameObject.SetActive(false);
            _buildingLevelDropdown.transform.parent.gameObject.SetActive(false);
        }

        _tileTypeText.text = tileType;
        _tileFluidTypeDropDown.value = (int)selectedTile.CurrentFluidType;
        _tileFluidLevelSlider.value = selectedTile.GetFluidLevel();

        _tilePanel.SetActive(true);
    }

    /// <summary>
    /// Set pause panel visiblity
    /// </summary>
    public void PausePanelVisible()
    {
        if (!_yourMapsPanel.activeInHierarchy && !_dialogPanel.activeInHierarchy)
        {
            _pausePanel.SetActive(!_pausePanel.activeInHierarchy);
            _pausePanel.transform.GetChild(0).gameObject.SetActive(true);
            _pausePanel.transform.GetChild(1).gameObject.SetActive(false);
            _pauseMenu.transform.GetChild(1).gameObject.SetActive(true);
            _yourMapsPanel.SetActive(false);
            _dialogPanel.SetActive(false);
            HideAllDialogPanels();
        }
        else
        {
            _yourMapsPanel.SetActive(false);
            _dialogPanel.SetActive(false);
            HideAllDialogPanels();
        }
    }

    /// <summary>
    /// Hide all map dialog panels
    /// </summary>
    public void HideAllDialogPanels()
    {
        _savePanel.SetActive(false);
        _deletePanel.SetActive(false);
        _newPanel.SetActive(false);
    }

    /// <summary>
    /// Prepopulate save map name input with current map name
    /// </summary>
    public void SetCurrentMapName()
    {
        _saveMapNameInput.text = MapSaver.CurrentFileName;
    }

    /// <summary>
    /// Save tileset with specified name
    /// </summary>
    public void SaveMap()
    {
        MapSaver.SaveTileSet(_saveMapNameInput.text, LevelEditor.IsDeveloperMode());
        StaticMapInfo.LevelEditorLevel = _saveMapNameInput.text;
    }
    public void PlayScene()
    {
        LevelEditor.StartScene();
    }

    public void EndScene()
    {
        LevelEditor.ReturnToEditor();
    }

    /// <summary>
    /// Fill load map dropdown options with saved map names
    /// </summary>
    public void PopulateMapDropdown()
    {
        List<MapSaveController.MapRefrence> mapNames = MapSaver.GetSavedMapNames();

        if (_mapsContentView.transform.childCount > 0)
        {
            for (int i = 0; i < _mapsContentView.transform.childCount; i++)
            {
                Destroy(_mapsContentView.transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < mapNames.Count; i++)
        {
            if (!mapNames[i].name.Contains("_TMP"))
            {
                GameObject mapListItem = Instantiate(_mapContentItemPrefab, _mapsContentView.transform);
                mapListItem.GetComponent<MapMenuItemScript>().SetInfo(mapNames[i]);
            }
        }
    }

    public void LoadMap(string mapName, bool IsResource)
    {
        _yourMapsPanel.SetActive(false);
        StaticMapInfo.LevelEditorLevel = mapName;
        StaticMapInfo.LoadIntoEditor(false);
        StaticMapInfo.SetLevelLoadData(mapName, false);
        LoadLevelSceneScript.instance.StartLoadLevel(StaticMapInfo.Level);
    }
    public void DeleteMap(string mapName)
    {
        MapSaver.DeleteTileSet(mapName);
        PopulateMapDropdown();
    }

    /// <summary>
    /// Create a new map using the specified size grid
    /// </summary>
    public void CreateMap()
    {
        LevelEditor.CreateNewMap(int.Parse(_newMapSizeXInput.text), int.Parse(_newMapSizeYInput.text));
        MapSaver.CurrentFileName = "";
        StaticMapInfo.LevelEditorLevel = "";
    }

    /// <summary>
    /// Set the grid size using the specified size
    /// </summary>
    public void UpdateGridSize()
    {
        //TileMap.SetGridSize(int.Parse(_currentMapSizeXInput.text), int.Parse(_currentMapSizeYInput.text));
    }

    /// <summary>
    /// Set new fluid level for the selected tile
    /// </summary>
    /// <param name="newLevel">New fluid level</param>
    public void SetFluidLevel(float newLevel)
    {
        Tile selectedTile = LevelEditor.CurrentSelectedTile;
        selectedTile.SetFluidLevel(newLevel, selectedTile.CurrentFluidType);
    }

    /// <summary>
    /// Set new fluid type for the selected tile
    /// </summary>
    /// <param name="newType">New fluid type</param>
    public void SetFluidType(int newType)
    {
        Tile selectedTile = LevelEditor.CurrentSelectedTile;
        selectedTile.SetFluidLevel(selectedTile.GetFluidLevel(), (Fluid.FluidType)newType);
    }

    /// <summary>
    /// Change the rock model to drop ore or energy crystals depending on the current rocks drop type
    /// </summary>
    public void ChangeRockDropType()
    {
        Tile selectedTile = LevelEditor.CurrentSelectedTile;
        int rockTypeId = selectedTile.GetComponent<RockScript>().GetID();

        if (_rockCrystalToggle.isOn != (rockTypeId == (int)Tile.TileTypeID.DirtEnergy || rockTypeId == (int)Tile.TileTypeID.HardRockEnergy || rockTypeId == (int)Tile.TileTypeID.LooseRockEnergy))
        {
            Dictionary<int, GameObject> tileDic = TileLib.GetTileList();
            GameObject newRockPrefab = tileDic[rockTypeId];

            switch (rockTypeId)
            {
                case (int)Tile.TileTypeID.Dirt:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.DirtEnergy];
                    break;
                case (int)Tile.TileTypeID.DirtEnergy:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.Dirt];
                    break;

                case (int)Tile.TileTypeID.LooseRock:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.LooseRockEnergy];
                    break;
                case (int)Tile.TileTypeID.LooseRockEnergy:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.LooseRock];
                    break;

                case (int)Tile.TileTypeID.HardRock:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.HardRockEnergy];
                    break;
                case (int)Tile.TileTypeID.HardRockEnergy:
                    newRockPrefab = tileDic[(int)Tile.TileTypeID.HardRock];
                    break;
            }

            TileMap.UpdateTile(selectedTile.X, selectedTile.Y, newRockPrefab);
            LevelEditor.CurrentSelectedTile = TileMap.GetTileAtPos(selectedTile.X, selectedTile.Y);
        }
    }

    /// <summary>
    /// Set the selected building level
    /// </summary>
    /// <param name="newLevel"></param>
    public void ChangeBuildingLevel(int newLevel)
    {
        LevelEditor.CurrentSelectedTile.GetComponent<Building>().BuildingLevel = _buildingLevelDropdown.value;
    }

    /// <summary>
    /// Clear the current tile, update level editor selected tile and redisplay tile properties panel
    /// </summary>
    public void ClearTile()
    {
        Tile selectedTile = LevelEditor.CurrentSelectedTile;
        if (selectedTile == null)
        {
            return;
        }
        TileMap.ClearTile(selectedTile.X, selectedTile.Y);

        LevelEditor.CurrentSelectedTile = TileMap.GetTileAtPos(selectedTile.X, selectedTile.Y);
        TilePanelVisible();
    }


    public void ShowTileToolTip(int tileIndex)
    {
        _descriptionText.text = _tileDescriptions[tileIndex];
    }

    public void ClearToolTip()
    {
        _descriptionText.text = "";
    }

    /// <summary>
    /// Hide all cancel buttons in the build menu after finishing building placement
    /// </summary>
    public void FinishSelect()
    {
        for (int i = 0; i < _tileContentView.transform.childCount; i++)
        {
            _tileContentView.transform.GetChild(i).GetComponent<TileMenuItemScript>().HideCancel();
        }
    }

}
