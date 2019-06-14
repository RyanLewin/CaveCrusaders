using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : MonoBehaviour
{
    TileLibrary _Lib = null;
    [SerializeField]
    TileMap3D _TileMap = null;
    GameObject _ToBuild = null;
    List<int> TileIds = null;
    GameObject TempInstance = null;
    bool rotateTile = false;
    WorldController _worldController;


    // Start is called before the first frame update
    void Start ()
    {
        _Lib = GetComponent<TileLibrary>();

        Dictionary<int, GameObject> t = _Lib.GetTileList();
        TileIds = new List<int>(t.Keys);
        TileIds.Sort();
        _worldController = WorldController.GetWorldController;
    }

    private void Update ()
    {
        SetBuild();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CancelBuild();
        }

        if (_ToBuild)
        {
            if (_ToBuild.GetComponent<Building>())
            {
                if (_ToBuild.GetComponent<Building>()._BuildCost <= _worldController._oreCount)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                        rotateTile = !rotateTile;
                    PlaceBuilding();
                }
                else
                {
                    _worldController.UIScript.ShowNotification("Not enough Ore!");
                    _ToBuild = null;
                }
            }
        }
    }

    public void UISetBuild (int id)
    {
        _ToBuild = _Lib.GetTilePrefab(TileIds[id + (int)Tile.TileTypeID.HQ]);
    }

    void SetBuild ()
    {
        for (int i = 0; i < TileIds.Count - (int)Tile.TileTypeID.Skip; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && !MenuScript.GamePaused)
            {
                //Garage4
                if (i >= 3)
                    _ToBuild = _Lib.GetTilePrefab(TileIds[i - 3 + (int)Tile.TileTypeID.Garage]);
                else
                    _ToBuild = _Lib.GetTilePrefab(TileIds[i + (int)Tile.TileTypeID.Skip]);

                if (!_ToBuild.GetComponent<Building>())
                {
                    CancelBuild();
                    break;
                }

                if (_worldController._oreCount < _ToBuild.GetComponent<Building>()._BuildCost
                    || (!_worldController._UseO2 && _ToBuild.GetComponent<Building>().GetID() == (int)Tile.TileTypeID.OxyGen))
                {
                    CancelBuild();
                }
                break;
            }
        }
    }

    void PlaceBuilding ()
    {
        MouseSelectionController.Get().SetMode(MouseSelectionController.CurrentSelectionMode.PlaceBuildings);
        Vector3 MouseWorldPos = new Vector3();
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Tile HitTile = null;
        if (Physics.Raycast(_ray, out hit))
        {
            MouseWorldPos = hit.point;
            Vector2 pos = _TileMap.WorldSpaceToGridSpace(MouseWorldPos);
            HitTile = _TileMap.GetTileAtPos((int)pos.x, (int)pos.y);
            //HitTile = hit.transform.GetComponent<Tile>();
            if (HitTile == null)
            {
                HitTile = hit.transform.GetComponentInParent<Tile>();
            }
            if (!HitTile)
            {
                //_worldController.UIScript.ShowNotification("Can't build here!");
                Destroy(TempInstance);
                _TileMap.ClearTempTile();
                return;
            }
            if (HitTile.GetID() != (int)Tile.TileTypeID.DefaultTile)
            {
                //_worldController.UIScript.ShowNotification("Can't build here!");
                Destroy(TempInstance);
                _TileMap.ClearTempTile();
                return;
            }


            Building toBuild = _ToBuild.GetComponent<Building>();

            if (toBuild)
            {
                if (toBuild._BuildingSize != null)
                {
                    for (int i = 0; i < toBuild._BuildingSize.Length; i++)
                    {
                        Tile tile = _TileMap.GetTileAtPos(HitTile.X + (int)toBuild._BuildingSize[i].x,
                                                          HitTile.Y + (int)toBuild._BuildingSize[i].y);
                        if (tile)
                        {
                            if (tile.GetID() != (int)Tile.TileTypeID.DefaultTile)
                            {
                                if (HitTile.gameObject != TempInstance)
                                {
                                    Destroy(TempInstance);
                                    _TileMap.ClearTempTile();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Destroy(TempInstance);
            _TileMap.ClearTempTile();
            return;
        }

        Vector3 rotation;
        if (rotateTile)
            rotation = new Vector3(0, 90, 0);
        else
            rotation = Vector3.zero;

        //if (TempInstance.GetComponent<Building>())
        //{
        //    if (TempInstance.GetComponent<Building>().GetID() == _ToBuild.GetComponent<Building>().GetID())
        //    {
        //        SetPlacement(HitTile, rotation);
        //        return;
        //    }
        //}
        
        _TileMap.UpdateTile_Temp(HitTile.X, HitTile.Y, _ToBuild, ref TempInstance, false);
        TempInstance.transform.rotation = Quaternion.Euler(rotation);
        //TempInstance.GetComponent<Building>()._BuildingObject.SetActive(false);
        //TempInstance.GetComponent<Building>()._PreviewObject.SetActive(true);
        //TempInstance.GetComponent<Building>()._Built = false;
        //TempInstance.transform.rotation = Quaternion.Euler(rotation);


        SetPlacement(HitTile, rotation);
    }

    void SetPlacement (Tile HitTile, Vector3 rotation)
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            _ToBuild.GetComponent<Building>()._Built = false;
            _TileMap.UpdateTile(HitTile.X, HitTile.Y, _ToBuild);
            //HitTile.GetComponent<Building>()._Built = false;
            Destroy(TempInstance);
            _TileMap.ClearTempTile();

            Building building = _TileMap.GetTileAtPos(HitTile.X, HitTile.Y).GetComponent<Building>();
            building.rotated = rotateTile;
            building.transform.rotation = Quaternion.Euler(rotation);

            WorldController.GetWorldController._oreCount -= building._BuildCost;
            WorldController.GetWorldController._levelStatsController.OreUsed(building._BuildCost);
            //WorldController.GetWorldController._buildings.Add(building);
            CreateTask(building);
            BuildMenuScript.instance.FinishBuild();
            MouseSelectionController.Get().SetMode(MouseSelectionController.CurrentSelectionMode.Select);
            if (!Input.GetKey(KeyCode.LeftShift) || _worldController._oreCount < building._BuildCost)
                _ToBuild = null;
        }
    }

    void CreateTask (Building building)
    {
        UnitTask tempTask = TaskLibrary.Get().CreateTask(UnitTask.TaskType.Build, building.transform.position, building.gameObject);
        building._BuildTask = tempTask;
        TaskList.AddTaskToGlobalTaskList(tempTask);
    }

    public void CancelBuild()
    {
        _ToBuild = null;
        MouseSelectionController.Get().SetMode(MouseSelectionController.CurrentSelectionMode.Select);
        BuildMenuScript.instance.FinishBuild();
        Destroy(TempInstance);
        _TileMap.ClearTempTile();
    }
}
