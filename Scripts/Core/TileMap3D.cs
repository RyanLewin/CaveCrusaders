using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//Handles everything relating to the Grid of Tiles.
//On Begin Play the Grid will be built 
public class TileMap3D : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField]
    int GridXSize = 10;
    [SerializeField]
    int GridYSize = 11;
    [SerializeField]
    float Step = 8.0f;
    float CurrentStep = 8.0f;
    [SerializeField]
    GameObject DefaultTileObject = null;
    [SerializeField]
    IGridNotify[] _ObjectsToNotify = { null };
    GameObject GridRoot = null;
    Tile[,] Grid;
    [Header("Debug")]
    [SerializeField]
    bool DEBUG_SeperateTiles = false;
    public int GridXSize_ { get => GridXSize; }
    public int GridYSize_ { get => GridYSize; }
    public float GridSpacing { get => CurrentStep; }
    const string GENTAG = "GEN";
    bool HasChanged = true;
    Tile InactiveTile = null;
    Vector3 StartPos = new Vector3(0, 0, 0);
    Vector3 GridCentre = new Vector3(0, 0, 0);
    [SerializeField]
    bool IsRunning = true;
    bool IsCurrentlyLoading = false;
    void OnValidate()
    {
        const int MaxGridSize = 75;
        GridXSize = Mathf.Clamp(GridXSize, 10, MaxGridSize);
        GridYSize = Mathf.Clamp(GridYSize, 10, MaxGridSize);
    }
    public bool IsGridInitalised()
    {
        return Grid != null;
    }
    public static bool IsLoading()
    {
        return WorldController._worldController.GetComponent<TileMap3D>().IsCurrentlyLoading;
    }
    public void NotifyLoading(bool state)
    {
        IsCurrentlyLoading = state;
    }
    public Vector3 GetGridCentre()
    {
        return GridCentre;
    }

    public bool IsSimulating()
    {
        return IsRunning;
    }

    public void SetSimulationState(bool active)
    {
        IsRunning = active;
    }
    /// <summary>
    /// Destroys the current Tile GameObject and Instantiates a new one at the specified position.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="NewTilePrefab">The GameObject Prefab that conatins the new tile a a Tile script</param>
    public void UpdateTile(int x, int y, GameObject NewTilePrefab, bool SurpressStart = false)
    {
        if (Grid == null || NewTilePrefab == null)
        {
            Debug.LogError("Grid not initialized");
            return;
        }

        GameObject NewTileGo = Instantiate(NewTilePrefab);
        if (NewTileGo.name.Contains("(Clone)"))
        {
            var n = NewTileGo.name.Split('(');
            NewTileGo.name = n[0];
        }
        //check
        NewTileGo.transform.position = Grid[x, y].transform.position;
        NewTileGo.transform.SetParent(GridRoot.transform);
        if (Grid[x, y] != null)
        {
            Grid[x, y].TileDestroy();
        }
        if (Application.isPlaying)
        {
            Destroy(Grid[x, y].gameObject);
        }
        else
        {
            DestroyImmediate(Grid[x, y].gameObject);
        }

        Grid[x, y] = NewTileGo.GetComponent<Tile>();

        if (Grid[x, y] == null)
        {
            Debug.LogError("The Tile Prefab did not have a Tile Script. All tile GameObjects need to have one");
            return;
        }
        if (Grid[x, y].GetComponent<Building>())
        {
            Vector2[] temp = Grid[x, y].GetComponent<Building>()._BuildingSize;
            if (temp.Length > 0)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    Tile OtherT = GetTileAtPos(x + (int)temp[i].x, y + (int)temp[i].y);
                    if (OtherT != null)
                    {
                        OtherT.SetID(Grid[x, y].GetID());
                        OtherT.tag = Grid[x, y].tag;
                        OtherT.IgnoreBuild = true;
                    }
                    else
                    {
                        //abort!
                        ClearTile(x, y);
                        return;
                    }
                }
            }
        }
        Grid[x, y].X = x;
        Grid[x, y].Y = y;
        Grid[x, y].gameObject.isStatic = true;
        try
        {
            if (!SurpressStart)
            {
                Grid[x, y].TileStart();
            }
        }
        catch { }
        if (NewTilePrefab == DefaultTileObject)
        {
            Grid[x, y].enabled = true;
            WorldController.GetWorldController._defultTile.Add(Grid[x, y]);
        }
        SetDirty();

    }

    //Used for the level editor hides the current tile and places another one in its place
    public void UpdateTile_Temp(int x, int y, GameObject NewTilePrefab, ref GameObject Instance, bool Reinit)
    {
        Tile t = GetTileAtPos(x, y);
        if (t == null)
        {
            return;
        }
        if (InactiveTile != t && InactiveTile != null)
        {
            InactiveTile.SetVisbility(true);
        }
        t.SetVisbility(false);
        InactiveTile = t;
        if (Reinit)
        {
            Destroy(Instance);
            Instance = null;
        }
        if (Instance == null)
        {
            Instance = Instantiate(NewTilePrefab);
            if (Instance.GetComponent<Building>())
            {
                Building building = Instance.GetComponent<Building>();
                building.SetID((int)Tile.TileTypeID.DefaultTile);
                building._BuildingObject.SetActive(false);
                building._PreviewObject.SetActive(true);
                building._Built = false;
            }

        }
        Instance.transform.SetPositionAndRotation(t.transform.position, t.transform.rotation);
        Instance.GetComponent<Tile>().X = t.X;
        Instance.GetComponent<Tile>().Y = t.Y;
    }

    public void ClearTempTile()
    {
        if (InactiveTile != null)
        {
            InactiveTile.SetVisbility(true);
            InactiveTile = null;
        }
    }

    public void ClearTile(int x, int y)
    {
        UpdateTile(x, y, DefaultTileObject);
    }

    public bool IsGridValid()
    {
        return Grid != null && GridRoot != null;
    }
    /// <summary>
    /// Gets the Tile at the specified grid position.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Tile Script on the Tile at the grid position</returns>
    public Tile GetTileAtPos(int x, int y)
    {
        if (x < 0 || y < 0 || x >= GridXSize || y >= GridYSize)
        {
            return null;
        }
        return Grid[x, y];
    }

    public Vector2Int WorldSpaceToGridSpace(Vector3 v)
    {
        Vector3 relativepos = v - StartPos;
        int x = Mathf.RoundToInt(relativepos.x / CurrentStep);
        int y = Mathf.RoundToInt(relativepos.z / CurrentStep);
        return new Vector2Int(x, y);
    }
    public Vector3 GridSpaceToWorldSpace(int x, int y)
    {
        return StartPos + new Vector3(x * CurrentStep, 0.0f, y * CurrentStep);
    }
    //Begin Internal Functions
    // Start is called before the first frame update
    void Start()
    {
        StaticMapInfo.Map = this;
#if UNITY_EDITOR
        GridTester tester = GetComponent<GridTester>();
        if (tester != null)
        {
            tester.BuildTest();
        }
#endif
        //DestroyGrid();
        //BuildTiles();
        //RockProximty();
    }

    // Update is called once per frame
    void Update()
    {
        if (HasChanged)
        {
            BroadCastUpdate();
            foreach (IGridNotify IGN in _ObjectsToNotify)
            {
                IGN.OnGridUpdate();
            }
            HasChanged = false;
        }
    }

    //todo: Async?
    public void BuildTiles()
    {
        OnValidate();
        if (DEBUG_SeperateTiles)
        {
            CurrentStep = Step + 4.0f;
        }
        else
        {
            CurrentStep = Step;
        }
        GridRoot = new GameObject();
        GridRoot.transform.SetParent(transform);
        GridRoot.tag = GENTAG;
        Grid = new Tile[GridXSize, GridYSize];
        for (int x = 0; x < GridXSize; x++)
        {
            for (int y = 0; y < GridYSize; y++)
            {
                GameObject Newobj = Instantiate(DefaultTileObject, StartPos + (new Vector3(x, 0, y) * CurrentStep), Quaternion.identity);
                Newobj.transform.SetParent(GridRoot.transform, true);
                Grid[x, y] = Newobj.GetComponent<Tile>();
                Grid[x, y].X = x;
                Grid[x, y].Y = y;
            }
        }
        GridCentre = new Vector3((GridXSize / 2) * CurrentStep, 0, (GridYSize / 2) * CurrentStep);
        GridCentre -= new Vector3(CurrentStep / 2, 0, CurrentStep / 2);
        //Once all Tiles are built
        BroadCastStart();
        BroadcastGridCreate();
        SetDirty();
    }

    public void BroadCastStart()
    {
        for (int x = 0; x < GridXSize; x++)
        {
            for (int y = 0; y < GridYSize; y++)
            {
                try
                {
                    Grid[x, y].TileStart();
                }
                catch (Exception e)
                {
                    Debug.Log("Error in TileStart " + e.Message);
                }
            }
        }
    }
    public void BroadcastGridCreate()
    {
        foreach (IGridNotify IGN in _ObjectsToNotify)
        {
            IGN.OnGridCreationFinished();
        }
    }
    public void BroadcastEditorPlay()
    {
        if (!IsGridInitalised())
        {
            return;
        }
        for (int x = 0; x < GridXSize; x++)
        {
            for (int y = 0; y < GridYSize; y++)
            {
                Grid[x, y].OnEditorPlay();
            }
        }
    }
    void BroadCastUpdate()
    {
        if (!IsGridInitalised())
        {
            return;
        }
        for (int x = 0; x < GridXSize; x++)
        {
            for (int y = 0; y < GridYSize; y++)
            {
                try
                {
                    Grid[x, y].OnGridUpdate();
                }
                catch (Exception E)
                {
                    Debug.LogWarning("Error: " + E.Message);
                }
            }

        }
    }

    public void SetGridSize(int x, int y)
    {
        //todo: validate
        GridXSize = x;
        GridYSize = y;
        OnValidate();
    }
    void DestroyAllTiles()
    {
        if (!IsGridInitalised())
        {
            return;
        }
        for (int x = 0; x < GridXSize; x++)
        {
            for (int y = 0; y < GridYSize; y++)
            {
                Grid[x, y].TileDestroy();
            }
        }
    }
    public void DestroyGrid()
    {
        DestroyAllTiles();
        foreach (Transform t in transform)
        {
            if (t.CompareTag(GENTAG))
            {
                GridRoot = t.gameObject;
            }
        }
        if (GridRoot != null)
        {
            if (Application.isPlaying)
            {
                Destroy(GridRoot);
            }
            else
            {
                DestroyImmediate(GridRoot);
            }
        }
        InactiveTile = null;
        SetDirty();
        foreach (IGridNotify IGN in _ObjectsToNotify)
        {
            IGN.OnGridDestruction();
        }
    }

    public void SetDirty()
    {
        HasChanged = true;
    }

    void RockProximty()
    {
        for (int x = 0; x < GridXSize_; x++)
        {
            for (int y = 0; y < GridYSize_; y++)
            {
                Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y);
                if (tile.tag == "RockTile")
                {
                    RockScript tileRockSript = tile.GetComponent<RockScript>();
                    tileRockSript.RockNorth = StaticMapInfo.isWall(x, y + 1);
                    tileRockSript.RockNorthEast = StaticMapInfo.isWall(x + 1, y + 1);
                    tileRockSript.RockEast = StaticMapInfo.isWall(x + 1, y);
                    tileRockSript.RockSouthEast = StaticMapInfo.isWall(x + 1, y - 1);
                    tileRockSript.RockSouth = StaticMapInfo.isWall(x, y - 1);
                    tileRockSript.RockSouthWest = StaticMapInfo.isWall(x - 1, y - 1);
                    tileRockSript.RockWest = StaticMapInfo.isWall(x - 1, y);
                    tileRockSript.RockNorthWest = StaticMapInfo.isWall(x - 1, y + 1);
                }
            }
        }
    }
    public Tile FindCell(Vector3 pos)
    {
        Vector2Int Gpos = WorldSpaceToGridSpace(pos);
        Tile t = GetTileAtPos(Gpos.x, Gpos.y);
        return t;
    }

}
