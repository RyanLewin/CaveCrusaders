using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Tile : IGridNotify
{
    protected int x;
    protected int y;
    [TileAttrib]
    public float _FluidLevel = 0.0f;//do not edit me!
    [TileAttrib]
    public Fluid.FluidType _CurrentType = Fluid.FluidType.None;//do not edit me!
    public Fluid.FluidType CurrentEdtiorVisType = Fluid.FluidType.None;//do not edit me!
    //Solid cells do not take Fluid damage and are impassable
    protected bool _IsSolid = false;
    //Tiles that will have OnFluidContact called 
    protected bool _IsDestructable = false;
    bool _IsSettled = false;
    int _SettleCount = 0;
    const int _SettleThreshold = 10;
    bool _ReceivedFluidMessage = false;
    FluidContactInfo _FluidContactMsg;
    [TileAttrib]
    public bool TestBool = false;
    public static float MaxFluid = 1.0f;
    bool bIsVisible = true;
    [TileAttrib]
    public bool _IsSourceBlock = false;
    [TileAttrib]
    public Fluid.FluidType SourceBlockType = Fluid.FluidType.None;
    [SerializeField]
    protected float _TileHealth = 0.0f;
    protected float _TileDurablity = 1.0f;
    [TileAttrib]
    [SerializeField]
    protected TileTypeID _TileType = TileTypeID.DefaultTile;
    [TileAttrib]
    protected TileTypeID _OldTileType;
    [TileAttrib]
    public bool IgnoreBuild = false;
    Renderer Mesh;
    [Header("Fluid")]
    Material DefaultMat;
    [HideInInspector]
    public Vector3 Thread_Cached_WorldPos;
    [HideInInspector]
    public bool IsContactingFluid = false;

    protected bool HadFluidBlock = false;
    protected bool _CanFluidEnter = false;
    List<UnitTask> TaskThatTargetThis = new List<UnitTask>();
    public void NotifyAboutTask(UnitTask t)
    {
        //clean up old refs;
        TaskThatTargetThis.Add(t);
    }
    float Factor = 0.0f;
    float TimeInFluid = 0.0f;

    public float GetFluidRenderingHeight()
    {
        return GetFluidLevel() * Factor;
    }
    public void CancelTilesTasks()
    {
        foreach (UnitTask t in TaskThatTargetThis)
        {
            foreach (Worker W in WorldController.GetWorldController._workers)
            {
                if (W.CurrentTask == t)
                {
                    W.CancelCurrentTask();
                }
            }
            if (t._taskType == UnitTask.TaskType.Build)
            {
                if (t._targetBuilding != null)
                {
                    t._targetBuilding.DestroyBuilding();
                }
            }
            t.DestroyTask();
            t.Invalidate();
        }
    }

    public bool IsSpecialFluidBlock()
    {
        return HadFluidBlock;
    }
    public virtual void SetID(int value)
    {
        //if (value < System.Enum.GetNames(typeof(TileTypeID)).Length)
        _OldTileType = (TileTypeID)GetID();
        _TileType = (TileTypeID)value;
    }
    public virtual int GetID()
    {
        if (_IsSourceBlock)
        {
            if (SourceBlockType == Fluid.FluidType.Lava)
            {
                return (int)TileTypeID.LavaTile;
            }
            else if (SourceBlockType == Fluid.FluidType.Water)
            {
                return (int)TileTypeID.WaterTile;
            }
        }
        return (int)_TileType;
    }
    public virtual int GetOldID()
    {
        return (int)_OldTileType;
    }
    public enum TileTypeID { DefaultTile, Dirt, LooseRock, HardRock, SolidRock, HQ, Skip, OxyGen, LavaBlockade, DirtEnergy, LooseRockEnergy, HardRockEnergy, LavaTile, OneWorkerTile, TwoWorkerTile, MosterTile, WaterTile, RubbleTile, Garage, Outpost, H2OConverter, AISpawn, Turret, PowerGen };
    public void TryToSettle()
    {
        _SettleCount++;
        if (_SettleCount > _SettleThreshold)
        {
            _IsSettled = true;
        }
    }

    public void UnSettle()
    {
        _IsSettled = false;
        _SettleCount = 0;
    }

    public bool IsCellSettled()
    {
        return _IsSettled;
    }

    public bool IsBlocked()
    {
        return _IsSolid;
    }

    public bool HasBuilding()
    {
        return _IsDestructable;
    }
    public bool CanFluidEnter()
    {
        return _CanFluidEnter;
    }
    public void FluidContact(FluidContactInfo f)
    {
        //Only one OnFluidContact is called per frame
        if (_ReceivedFluidMessage)
        {
            return;
        }
        _FluidContactMsg = f;
        _ReceivedFluidMessage = true;
        OnFluidContact(_FluidContactMsg);
    }

    protected virtual void OnFluidContact(FluidContactInfo f)
    {
        if (_TileHealth > 0.0f)
        {
            if (f.ContactedFluid != null)
            {
                _TileHealth -= f.ContactedFluid.DamageToTiles * Time.deltaTime;
            }
            else
            {
                _TileHealth -= 1.0f;
            }
        }
        else
        {
            TileMap3D mpa = WorldController._worldController.GetComponent<TileMap3D>();
            mpa.ClearTile(x, y);
        }
    }

    // Start is called before the first frame update

    void Start()
    {
        Mesh = GetComponent<Renderer>();
        if (Mesh != null)
        {
            DefaultMat = Mesh.material;
        }
        if (!WorldController.IsPlaying() && SourceBlockType != Fluid.FluidType.None)
        {
            Editor_SetFluidVisState(SourceBlockType);
        }
    }
    public void TileStart()
    {
        if (!WorldController.IsPlaying())
        {
            Editor_SetFluidVisState(SourceBlockType);
        }
        Thread_Cached_WorldPos = transform.position;
        OnTileStart();
        _TileHealth = _TileDurablity;
        if (this is Building)
        {
            if (GetComponent<Building>()._barsUpdater != null)
            {
                GetComponent<Building>()._barsUpdater._maxHealth = _TileHealth;
            }
        }

    }
    //Called after TileMap is ready
    protected virtual void OnTileStart()
    {

    }
    // Update is called once per frame
    void Update()
    {
        _ReceivedFluidMessage = false;
        if (_IsSourceBlock)
        {
            Factor = 1.0f;
        }
        else
        {
            if (GetFluidLevel() > FluidEngine.MIN_SIM_VALUE)
            {
                TimeInFluid += Time.deltaTime;
                Factor = TimeInFluid / FluidEngine.TileFillTime;
                Factor = Mathf.Clamp(Factor, 0, 1.0f);
            }
        }

        TileUpdate();

    }

    protected virtual void TileUpdate()
    {

    }

    public int X
    {
        set
        {
            x = value;
        }
        get
        {
            return x;
        }
    }

    public int Y
    {
        set
        {
            y = value;
        }
        get
        {
            return y;
        }
    }

    public Fluid.FluidType CurrentFluidType { get => _CurrentType; }

    public float GetFluidLevel()
    {
        return _FluidLevel;
    }

    public void SetFluidLevel(float level, Fluid.FluidType Type)
    {
        //todo: fluid interactions
        _CurrentType = Type;
        _FluidLevel = level;
        _FluidLevel = Mathf.Min(_FluidLevel, MaxFluid);
        if (_FluidLevel > FluidEngine.MIN_SIM_VALUE + FluidEngine.TileVisiblityThreshold && CurrentFluidType == Fluid.FluidType.Lava && WorldController.IsPlaying())
        {
            Internal_SetVisbility(false);
        }
        else
        {
            Internal_SetVisbility(bIsVisible);
        }

    }

    public void SetVisbility(bool state)
    {
        bIsVisible = state;
        Internal_SetVisbility(bIsVisible);
    }

    void Internal_SetVisbility(bool state)
    {
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
        {
            r.enabled = state;
        }
    }
    public void TileDestroy()
    {
        OnTileDestroy();
    }
    protected virtual void OnTileDestroy()
    {

    }

    public void Editor_SetFluidVisState(Fluid.FluidType Type)
    {
        if (Mesh == null)
        {
            return;
        }
        if (CurrentEdtiorVisType == Type)
        {
            return;
        }
        //if (CurrentEdtiorVisType == Fluid.FluidType.None)
        //{
        //    if (!_IsSourceBlock && GetFluidLevel() < 0.01f)
        //    {
        //        return; 
        //    }
        //}
        CurrentEdtiorVisType = Type;
        FluidEngine e = FluidEngine.Get();
        if (Type == Fluid.FluidType.Lava)
        {
            Mesh.material = e.LavaMat;
        }
        else if (Type == Fluid.FluidType.Water)
        {
            Mesh.material = e.WaterMat;
        }
        else
        {
            Mesh.material = DefaultMat;
        }
    }
    //called when the gamemode is set to play 
    //used to clear editor state
    public virtual void OnEditorPlay()
    {
        Editor_SetFluidVisState(Fluid.FluidType.None);
    }
}
