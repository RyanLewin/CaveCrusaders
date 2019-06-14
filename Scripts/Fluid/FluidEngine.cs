//#define DEBUGFLOW
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Profiling;

public class FluidEngine : IGridNotify
{
    struct CellDelta
    {
        public float Level;
        public Fluid.FluidType Type;
    }
    public const float MIN_SIM_VALUE = 0.0001f;

    [Header("Setup")]
    [SerializeField]
    FluidParticleSystemManager Particles;

    [Header("Config")]
    [SerializeField]
    float TileVisiblityThreshold_Proxy = 0.2f;
    [SerializeField]
    float FillTime_Proxy = 10.0f;
    [SerializeField]
    float _UpdateRate = 10.0f;
    [SerializeField]
    int _SolverIterations = 1;
    [SerializeField]
    float MinFlowLevelLava = 0.1f;
    [SerializeField]
    float MinFlowLevelWater = 0.0f;
    [SerializeField]
    float _GlobalMaxFlow = 5.0f;
    [Header("Performance")]
    [ReadOnly, SerializeField]
    float CellsPerSecond = 0;

    [Header("Debug")]
    [SerializeField]
    bool DebugRender = false;

    TileMap3D _TileMap = null;
    FluidLibrary _Lib = null;
    public static float TileVisiblityThreshold = 0.1f;
    public static float TileFillTime = 10.0f;
    CellDelta[,] _Diffs;
    float _UpdateCoolDown = 0.1f;
    float _CurrentCooldown = 0.0f;
    CustomSampler Samples;
    public int TilesLava = 0;
    public int TilesWater = 0;
#if DEBUGFLOW
    [Header("DEBUG")]
    [SerializeField]
    float _DeltaE = 0.0f;
    [SerializeField]
    float _Min = 0.0f;
    [SerializeField]
    float _Max = 0.0f;
#endif
    public Fluid.FluidType DebugType = Fluid.FluidType.Lava;
    public Vector2Int TargetPos = new Vector2Int(7, 5);
    public bool DEBUG_ADDFLuid = false;
    public Material LavaMat;
    public Material WaterMat;
    //SetMinFlow
    static FluidEngine Instance;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Samples = CustomSampler.Create("Fluid Update");
#endif

        _TileMap = transform.parent.GetComponent<TileMap3D>();
        _Lib = GetComponent<FluidLibrary>();

        UpdateScheduling();
        BuildDataStrcutres();
        Instance = this;
        if (WaterMat != null)
        {
            WaterMat = new Material(WaterMat);
            WaterMat.SetFloat("Vector1_FF411F1A", 1.0f);
        }
    }
    public static FluidEngine Get()
    {
        return Instance;
    }
    void BuildDataStrcutres()
    {
        _Diffs = new CellDelta[_TileMap.GridXSize_, _TileMap.GridYSize_];
        UnsettleAll();
    }
    void UnsettleAll()
    {
        if (!_TileMap.IsGridInitalised())
        {
            return;
        }
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                Tile t = _TileMap.GetTileAtPos(x, y);
                if (t != null)
                {
                    t.UnSettle();
                }
            }
        }
    }
    void UpdateScheduling()
    {
        _UpdateCoolDown = 1.0f / _UpdateRate;
        int cellcount = _TileMap.GridXSize_ * _TileMap.GridYSize_;
        CellsPerSecond = cellcount / _UpdateCoolDown;
        Debug.Log("Fluid: " + cellcount + " cells need to update in " + _UpdateCoolDown + "s So " + CellsPerSecond + " Cells/s");
    }

    public float RemoveFluidFromCell(int x, int y, float amt, Fluid.FluidType type)
    {
        Tile t = _TileMap.GetTileAtPos(x, y);
        if (t != null)
        {
            if (t.CurrentFluidType != type)
            {
                return 0.0f;
            }
            float RemovedAmt = Mathf.Min(amt, t.GetFluidLevel());
            AddFluidToTile(t, -RemovedAmt, type);
            return RemovedAmt;
        }
        return 0.0f;
    }

    public void Test()
    {
        Tile t = _TileMap.GetTileAtPos(TargetPos.x, TargetPos.y);
        if (t != null)
        {
            t.SetFluidLevel(1, DebugType);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!_TileMap.IsGridInitalised())
        {
            return;
        }
#if UNITY_EDITOR
        _UpdateCoolDown = 1.0f / _UpdateRate;
#endif
#if UNITY_EDITOR
        Samples.Begin();
#endif
        if (!_TileMap.IsSimulating())
        {
            EditorSimulate();
#if UNITY_EDITOR
            Samples.End();
#endif
            return;
        }
        TileVisiblityThreshold = TileVisiblityThreshold_Proxy;
        TileFillTime = FillTime_Proxy;
        _CurrentCooldown -= Time.deltaTime;
        if (_CurrentCooldown > 0.0f)
        {
#if UNITY_EDITOR
            Samples.End();
#endif
            return;
        }
        _CurrentCooldown = _UpdateCoolDown;
        for (int i = 0; i < _SolverIterations; i++)
        {
            Simulate();
        }
        if (DEBUG_ADDFLuid)
        {
            Test();
        }
#if UNITY_EDITOR
        Samples.End();
#endif
    }

    void EditorSimulate()
    {
        TilesLava = 0;
        TilesWater = 0;
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                Tile TargetT = _TileMap.GetTileAtPos(x, y);
                if (TargetT._IsSourceBlock)
                {
                    //TargetT.set
                    TargetT.Editor_SetFluidVisState(TargetT.SourceBlockType);

                    if (TargetT.CurrentFluidType == Fluid.FluidType.Lava)
                    {
                        TilesLava++;
                    }
                    else if (TargetT.CurrentFluidType == Fluid.FluidType.Water)
                    {
                        TilesWater++;
                    }
                    TargetT.SetFluidLevel(Tile.MaxFluid, TargetT.SourceBlockType);
                }
                else if (TargetT.GetFluidLevel() > 0.01f)
                {
                    TargetT.Editor_SetFluidVisState(TargetT.CurrentFluidType);
                }
                else
                {
                    TargetT.Editor_SetFluidVisState(Fluid.FluidType.None);
                }

                //else
                //{
                //    TargetT.IsContactingFluid = true;
                //    TargetT.SetFluidLevel(0, Fluid.FluidType.None);
                //}
            }
        }
    }

    void Clear()
    {
        TilesLava = 0;
        TilesWater = 0;
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                try
                {
                    _Diffs[x, y].Level = 0.0f;
                    Tile t = _TileMap.GetTileAtPos(x, y);
                    if (t != null)
                    {
                        _Diffs[x, y].Type = t.CurrentFluidType;
                        t.IsContactingFluid = false;
                        if (t.CurrentFluidType == Fluid.FluidType.Lava)
                        {
                            t.enabled = true;
                            TilesLava++;
                        }
                        else if (t.CurrentFluidType == Fluid.FluidType.Water)
                        {
                            t.enabled = true;
                            TilesWater++;
                        }
                    }
                }
                catch
                {
                    _Diffs = new CellDelta[_TileMap.GridXSize_, _TileMap.GridYSize_];
                }
            }
        }
    }

    void ComputeCellFlow(int x, int y)
    {
        Tile TargetT = _TileMap.GetTileAtPos(x, y);
        if (TargetT._IsSourceBlock)
        {
            TargetT.SetFluidLevel(Tile.MaxFluid, TargetT.SourceBlockType);
            UnsettleNear(x, y);
        }
        if (TargetT.CurrentFluidType == Fluid.FluidType.None)
        {
            return;
        }
        if (TargetT.IsCellSettled())
        {
            return;
        }
        Fluid Props = _Lib.GetFluid(TargetT.CurrentFluidType);
        float CurrentLevel = TargetT.GetFluidLevel();
        if (CurrentLevel < MIN_SIM_VALUE)
        {
            return;
        }
        if (TargetT.CurrentFluidType == Fluid.FluidType.Lava)
        {
            if (CurrentLevel < MinFlowLevelLava)
            {
                return;
            }
        }
        else
        {
            if (CurrentLevel < MinFlowLevelWater)
            {
                return;
            }
        }
        float StartLevel = CurrentLevel;
        FloodDir(x + 1, y, x, y, ref CurrentLevel, Props);
        FloodDir(x - 1, y, x, y, ref CurrentLevel, Props);
        FloodDir(x, y + 1, x, y, ref CurrentLevel, Props);
        FloodDir(x, y - 1, x, y, ref CurrentLevel, Props);
        if (StartLevel == CurrentLevel)
        {
            TargetT.TryToSettle();
        }
        else
        {
            UnsettleNear(x, y);
        }
    }
    public float AddFluidToTile(Tile t, float Add, Fluid.FluidType type)
    {
        if (type == Fluid.FluidType.Lava && t.GetFluidLevel() == 0)
        {
            if (WorldController.GetWorldController._soundManager.SpawnLavaSource(t.transform.position))
            {
                Instantiate(StaticMapInfo.RockModleHolder.LavaSound, new Vector3(t.transform.position.x, t.transform.position.y, t.transform.position.z), new Quaternion(0, 0, 0, 0));
            }
        }
        if (t._IsSourceBlock)
        {
            return 0.0f;
        }
        float level = t.GetFluidLevel() + Add;
        if (type == Fluid.FluidType.Lava && t.CurrentFluidType == Fluid.FluidType.Water)
        {
            //this is a water cell with lava being added.
            level = Add;
            type = Fluid.FluidType.Lava;
            Particles.NotifyFluidInteraction(t.gameObject.transform.position, 1.0f);
        }
        else if (type == Fluid.FluidType.Water && t.CurrentFluidType == Fluid.FluidType.Lava)
        {
            //this is a lava cell with water being added.
            level = t.GetFluidLevel();
            type = Fluid.FluidType.Lava;
            Particles.NotifyFluidInteraction(t.gameObject.transform.position, 1.0f);
        }
        t.SetFluidLevel(level, type);
        return level;
    }
    void Simulate()
    {
#if DEBUGFLOW
        _Min = 1000.0f;
        _Max = 0.0f;
#endif
        Clear();
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                ComputeCellFlow(x, y);
            }
        }
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                if (_Diffs[x, y].Level != 0.0f)
                {
                    Tile t = _TileMap.GetTileAtPos(x, y);
#if false
                    if (_Diffs[x, y].Level > 0.0f && _Diffs[x, y].Type == Fluid.FluidType.None)
                    {
                        Debug.Log("The levels of none fluid are rising, RUN!");
                    }
#endif
                    AddFluidToTile(t, _Diffs[x, y].Level, _Diffs[x, y].Type);
                }
            }
        }
#if DEBUGFLOW
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridXSize_; y++)
            {
                Tile t = _TileMap.GetTileAtPos(x, y);
                if (t.IsBlocked() || t.HasBuilding())
                {
                    continue;
                }
                float CurrentLevel = t.GetFluidLevel();
                _Min = Mathf.Min(_Min, CurrentLevel);
                _Max = Mathf.Max(_Max, CurrentLevel);
            }
        }
        _DeltaE = _Max - _Min;
#endif
    }

    void UnsettleCell(int x, int y)
    {
        Tile t = _TileMap.GetTileAtPos(x, y);
        if (t != null)
        {
            t.UnSettle();
        }
    }

    void UnsettleNear(int x, int y)
    {
        UnsettleCell(x + 1, y);
        UnsettleCell(x - 1, y);
        UnsettleCell(x, y + 1);
        UnsettleCell(x, y - 1);
    }

    void FloodDir(int x, int y, int originx, int originy, ref float CurrentLevel, Fluid f)
    {
        if (CanFlowToCell(x, y))
        {
            //try and flow into a build and damage it!
            if (TryFlowIntoBuilding(x, y, f))
            {
                return;//building is still Holding ... for now
            }
            if (CurrentLevel < MIN_SIM_VALUE)
            {
                _Diffs[x, y].Level -= CurrentLevel;
                return;
            }
            Tile flowtarget = _TileMap.GetTileAtPos(x, y);
            float flow = (CurrentLevel - flowtarget.GetFluidLevel()) / 4.0f;
            //min flow
            if (f._FlowRate > MIN_SIM_VALUE)
            {
                flow *= f._FlowRate;
            }

            flow = Mathf.Max(flow, 0.0f);
            if (flow > Mathf.Min(_GlobalMaxFlow, CurrentLevel))
            {
                flow = Mathf.Min(_GlobalMaxFlow, CurrentLevel);
            }

            if (flowtarget.GetFluidLevel() + flow > Tile.MaxFluid)
            {
                flow = Mathf.Min(flow, Tile.MaxFluid - flowtarget.GetFluidLevel());
            }
            if (flow > 0.0f)
            {
                CurrentLevel -= flow;
                _Diffs[originx, originy].Level -= flow;
                _Diffs[x, y].Level += flow;
                Tile origin = _TileMap.GetTileAtPos(originx, originy);
                if (origin != null)
                {
                    _Diffs[x, y].Type = origin.CurrentFluidType;
                }
                //unsettle
                UnsettleCell(x, y);
            }
        }
    }

    bool TryFlowIntoBuilding(int x, int y, Fluid f)
    {
        Tile TargetT = _TileMap.GetTileAtPos(x, y);
        if (!TargetT.HasBuilding())
        {
            return false;
        }

        FluidContactInfo FCI;
        FCI.ContactedFluid = f;
        TargetT.FluidContact(FCI);
        TargetT.IsContactingFluid = true;
        //fluid can enter this building 
        if (TargetT.CanFluidEnter() && f.GetFluidType() == Fluid.FluidType.Water)
        {
            return false;
        }
        return TargetT.HasBuilding();
    }

    bool CanFlowToCell(int x, int y)
    {
        Tile TargetT = _TileMap.GetTileAtPos(x, y);
        if (TargetT == null)
        {
            return false;
        }
        if (TargetT.IsBlocked())
        {
            TargetT.IsContactingFluid = true;
            return false;
        }
        return true;
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        RenderFluidDebug();
    }

    void RenderFluidDebug()
    {
        if (!DebugRender)
        {
            return;
        }
        if (_TileMap == null)
        {
            return;
        }
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                Tile t = _TileMap.GetTileAtPos(x, y);
                if (t == null || t.IsBlocked() || t.HasBuilding() || t.CurrentFluidType == Fluid.FluidType.None)
                {
                    continue;
                }
                Vector3 Pos = t.transform.position;
                Pos.y = 5.0f;
                float FluidLevel = t.GetFluidLevel();
                string label = t.CurrentFluidType.ToString() + " " + FluidLevel.ToString("0.0");
                Handles.Label(Pos, label);
            }
        }
    }

    public override void OnGridCreationFinished()
    {
        UpdateScheduling();
        BuildDataStrcutres();

    }

    public override void OnGridUpdate()
    {
        UnsettleAll();
    }
#endif
}

