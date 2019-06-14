using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//todo: Drag placement
//allow pre placed buildings in non dev?
public class LevelEditor : MonoBehaviour
{
    enum CurrentMode { Placement, Edit, None };
    [SerializeField]
    CurrentMode EditMode = CurrentMode.None;
    Tile CurrentSelection = null;

    TileMap3D _TileMap = null;
    GameObject CurrentPrefab = null;
    Tile HitTile = null;
    bool rotated = false;
    GameObject TempInstance = null;
    bool DidJustChangePlacementTile = false;
    TileLibrary Lib = null;
    [SerializeField]
    int alphaMinus = 0;
    List<int> TileIds = null;
    MapSaveController MSC;
    [SerializeField]
    GameObject UI, PlayControl;

    int MinSquareSize = 8;
    int MaxSquareSize = 75;
    // Start is called before the first frame update
    void Start()
    {
        _TileMap = WorldController.GetWorldController.TileMap;
        Lib = WorldController.GetWorldController.GetComponentInChildren<TileLibrary>();
        MSC = WorldController.GetWorldController.GetComponentInChildren<MapSaveController>();
        Dictionary<int, GameObject> t = Lib.GetTileList();
        TileIds = new List<int>(t.Keys);
        TileIds.Sort();

    }

    public Tile CurrentSelectedTile
    {
        get => CurrentSelection;
        set => CurrentSelection = value;
    }
    //certain properties are only available to developers 
    public static bool IsDeveloperMode()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (WorldController.IsPlaying())
        {
            return;
        }
        TickInput();
    }

    void TickInput()
    {
        if (Input.GetMouseButtonDown(0) && EditMode == CurrentMode.None && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(_ray, out hit))
            {
                CurrentSelection = hit.transform.GetComponent<Tile>();
                if (CurrentSelection == null)
                {
                    CurrentSelection = hit.transform.GetComponentInParent<Tile>();
                }
                LevelEditorUIScript.instance.TilePanelVisible();
            }
            else
            {
                CurrentSelection = null;
            }
        }

        for (int i = 0; i < TileIds.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 - alphaMinus + i) && !MenuScript.GamePaused)
            {
                CurrentPrefab = Lib.GetTilePrefab(TileIds[i]);
                DidJustChangePlacementTile = true;
                EditMode = CurrentMode.Placement;
                break;
            }
        }

        if (EditMode == CurrentMode.Placement && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotated = !rotated;
            }
            Vector3 MouseWorldPos = new Vector3();
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Tile CurrnetHitTile = null;
            if (Physics.Raycast(_ray, out hit))
            {
                MouseWorldPos = hit.point;
                CurrnetHitTile = hit.transform.GetComponent<Tile>();
                if (CurrnetHitTile == null)
                {
                    CurrnetHitTile = hit.transform.GetComponentInParent<Tile>();
                }
            }
            HitTile = CurrnetHitTile;
            if (HitTile != null)
            {
                _TileMap.UpdateTile_Temp(HitTile.X, HitTile.Y, CurrentPrefab, ref TempInstance, DidJustChangePlacementTile);
                TempInstance.transform.rotation = rotated ? Quaternion.Euler(new Vector3(0, 90, 0)) : new Quaternion();
            }
            if (Input.GetMouseButton(0) && HitTile != null)
            {
                Tile currentt = _TileMap.GetTileAtPos(HitTile.X, HitTile.Y);
                if (currentt != null)
                {
                    if (currentt.GetID() == CurrentPrefab.GetComponent<Tile>().GetID())
                    {
                        return;
                    }
                }
                //Debug.Log("palced tile");
                _TileMap.UpdateTile(HitTile.X, HitTile.Y, CurrentPrefab);
                Tile t = _TileMap.GetTileAtPos(HitTile.X, HitTile.Y);
                if (t.GetComponent<Building>())
                {
                    Building building = t.GetComponent<Building>();
                    building._Built = true;
                    building.rotated = rotated;
                    building.transform.rotation = rotated ? Quaternion.Euler(new Vector3(0, 90, 0)) : new Quaternion();
                }
                StaticMapInfo.RockProximty();
                _TileMap.SetDirty();
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            if (StaticMapInfo.LevelEditorLevel != "")
            {
                MSC.SaveTileSet(StaticMapInfo.LevelEditorLevel, IsDeveloperMode());
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            CurrentSelection = null;
            StopPlacement();
            EditMode = CurrentMode.None;
        }
        DidJustChangePlacementTile = false;
    }

    public void UpdateMode(WorldController.CurrentGameMode state)
    {
        UI.SetActive(state == WorldController.CurrentGameMode.Editor);
        PlayControl.SetActive(state != WorldController.CurrentGameMode.Play);
    }

    void StopPlacement()
    {
        CurrentPrefab = null;
        Destroy(TempInstance);
        EditMode = CurrentMode.None;
        _TileMap.ClearTempTile();
    }

    public bool CreateNewMap(int x, int y)
    {
        _TileMap.DestroyGrid();
        _TileMap.SetGridSize(x, y);
        _TileMap.BuildTiles();
        return true;
    }
    //UI
    public void SelectTileToPlace(int index)
    {
        //todo: check
        CurrentPrefab = Lib.GetTilePrefab(TileIds[index]);
        DidJustChangePlacementTile = true;
        Destroy(TempInstance);
        EditMode = CurrentMode.Placement;
    }

    public void AbortPlace()
    {
        StopPlacement();
    }

    public void Deselect()
    {
        CurrentSelection = null;
    }
    public const string EditorLevel = "LEVELEDTIOR_TMP";
    public void StartScene()
    {
        //save the map to a temp area and restart the scene
        MSC.SaveTileSet(EditorLevel, false);
        StaticMapInfo.SetLevelLoadData(EditorLevel, false);
        StaticMapInfo.LoadIntoEditor(true);
        LoadLevelSceneScript.instance.StartLoadLevel(StaticMapInfo.Level, false);
    }

    public void ReturnToEditor()
    {
        StaticMapInfo.LoadIntoEditor(false);
        StaticMapInfo.SetLevelLoadData(EditorLevel, false);
        LoadLevelSceneScript.instance.StartLoadLevel(StaticMapInfo.Level);
    }
    public void ClearMap()
    {
        _TileMap.DestroyGrid();
        _TileMap.BuildTiles();
    }
}
