//#define DEBUG_RENDER 
//#define USE_CULLING
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using B83.MeshHelper;
using UnityEngine.Profiling;

public class FluidRenderer : IGridNotify
{
    [System.Serializable]
    public struct InstancedFluidData
    {
        [HideInInspector]
        public Fluid.FluidType RenderType;
        public float SourceClamp;
        public float OriginClamp;
        public float SoftSkinDistance;
    }
    [Header("Config")]
    [SerializeField]
    float FluidZeroOffset = 200.0f;
    [SerializeField]
    float FluidBuildingZeroOffset = 200.0f;
    [SerializeField]
    int CellDevisions = 2;
    [SerializeField]
    InstancedFluidData IFData;

    [SerializeField]
    Fluid.FluidType FluidToRender = Fluid.FluidType.Lava;
    [Header("Setup")]
    [SerializeField]
    MeshFilter TargetMeshFilter = null;
    [SerializeField]
    TileMap3D TileMap = null;
    [SerializeField]
    FluidEngine Engine = null;
    [SerializeField]
    bool WeldVerts = true;
    [Header("Performance")]
    [SerializeField]
    bool UseJobSystem = false;

    [SerializeField]
    int BatchCount = 16;
    [SerializeField]
    int UpdateNormalsEveryFrames = 5;

    [Header("Debug")]
    [SerializeField]
    bool RefreshTransfrom = false;
    [ReadOnly, SerializeField]
    int VertexCount = 0;
    [ReadOnly, SerializeField]
    int TriangleCount = 0;
    [SerializeField]
    bool ForceUpdate = false;
#if USE_CULLING
    [SerializeField]
    bool DisableCulling = false;
#endif
    JobHandle Handle;
#if DEBUG_RENDER

    [SerializeField]
    bool DisableDiagonals = false;
    [SerializeField]
    bool DisableSmoothTO = false;
#endif
    FluidRenderJob RenderJob;
    Transform THREAD_transfrom;
    NativeArray<Vector3> vertices;
    NativeArray<Vector3> PreTransfromedVerts;
    Mesh mesh;
    static FluidRenderer Instance = null;
#if USE_CULLING
    //culling
    [Header("Culling")]
    [SerializeField]
    bool UseCullingJob = false;
    [SerializeField]
    float CullingPeriod = 0.0f;
    float currentTime = 0.0f;
    [SerializeField]
    float CulledPC = 0.0f;
    [SerializeField]
    bool CullingStats = false;

    FluidCullJob CullingJob;
    NativeArray<int> CullFlags;
#endif
    CustomSampler Samples;
    Vector2Int[] Directions;
    int CellCountLast = 0;
    AG_Profiler MeshGenTimer = new AG_Profiler();
    const int SimpleDirectionsLength = 4;
    bool DisableSkinning = false;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        THREAD_transfrom = transform;
        transform.parent.position = Vector3.zero;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Samples = CustomSampler.Create("Fluid Render (" + FluidToRender.ToString() + ")");
#endif
        Directions = new Vector2Int[8];
        Directions[0] = new Vector2Int(0, 1);
        Directions[1] = new Vector2Int(1, 0);
        Directions[2] = new Vector2Int(0, -1);
        Directions[3] = new Vector2Int(-1, 0);

        Directions[4] = new Vector2Int(1, 1);
        Directions[5] = new Vector2Int(1, -1);
        Directions[6] = new Vector2Int(-1, 1);
        Directions[7] = new Vector2Int(-1, -1);
    }

    private void OnDestroy()
    {
        Handle.Complete();
        if (vertices.IsCreated)
        {
            vertices.Dispose();
            PreTransfromedVerts.Dispose();
        }
#if USE_CULLING
        if (CullFlags.IsCreated)
        {
            CullFlags.Dispose();
        }
#endif
    }

    float MaxPos = 100;
    Vector2 GetUvAtPos(Vector3 pos)
    {
        return new Vector2(pos.x / 100, pos.z / 100);
    }
    private void CreateQuad(Vector3 CellRoot, float halfstep, Vector2 Pos, ref MeshArrays Array)
    {
        int TriIndex = Array.TriIndex;
        int i = Array.VertIndex;

        Array.Verts[i] = CellRoot + new Vector3(halfstep, 0, halfstep);
        Array.Verts[i + 1] = CellRoot + new Vector3(-halfstep, 0, halfstep);
        Array.Verts[i + 2] = CellRoot + new Vector3(halfstep, 0, -halfstep);
        Array.Verts[i + 3] = CellRoot + new Vector3(-halfstep, 0, -halfstep);

        Array.normals[i] = Vector3.up;
        Array.normals[i + 1] = Vector3.up;
        Array.normals[i + 2] = Vector3.up;
        Array.normals[i + 3] = Vector3.up;
        //  Lower left triangle.
        Array.tri[TriIndex + 0] = i + 0;
        Array.tri[TriIndex + 1] = i + 2;
        Array.tri[TriIndex + 2] = i + 1;

        //  Upper right triangle.   
        Array.tri[TriIndex + 3] = i + 2;
        Array.tri[TriIndex + 4] = i + 3;
        Array.tri[TriIndex + 5] = i + 1;

        Array.TriIndex += 6;
        Array.VertIndex += 4;
    }
    struct MeshArrays
    {
        public Vector3[] Verts;
        public int[] tri;
        public Vector3[] normals;
        public int VertIndex;
        public int TriIndex;
        public MeshArrays(int Tilecount, int VertsPerCel)
        {
            int TotalVertCount = Tilecount * VertsPerCel;
            int TriangleCount = Tilecount * 2;
            tri = new int[TriangleCount * 3];
            normals = new Vector3[TotalVertCount];
            Verts = new Vector3[TotalVertCount];
            VertIndex = 0;
            TriIndex = 0;
        }
    }

    Mesh CloneMesh(Mesh target)
    {
        Mesh NewMesh = new Mesh();
        NewMesh.vertices = target.vertices;
        NewMesh.uv = target.uv;
        NewMesh.triangles = target.triangles;
        NewMesh.normals = target.normals;
        return NewMesh;
    }

    Mesh BuildOrGetMesh()
    {
        if (Instance.mesh != null)
        {
            return CloneMesh(Instance.mesh);
        }
        int QuadCount = TileMap.GridXSize_ * TileMap.GridYSize_ * CellDevisions * CellDevisions;
        int VertexPerQuad = 4;

        float Step = TileMap.GridSpacing;
        float halfstep = Step / 2;
        float SubStep = Step / CellDevisions;
        Vector3 StartPoint = new Vector3(-SubStep / 2, 0, -SubStep / 2);
        MeshArrays Array = new MeshArrays(QuadCount, VertexPerQuad);
        MaxPos = TileMap.GridXSize_ * TileMap.GridYSize_ * Step;

        for (int x = 0; x < TileMap.GridXSize_ * CellDevisions; x++)
        {
            for (int y = 0; y < TileMap.GridYSize_ * CellDevisions; y++)
            {
                //smooth normals
                Vector3 CellRoot = StartPoint + new Vector3(x * SubStep, 0, y * SubStep);
                CreateQuad(CellRoot, SubStep / 2, new Vector2(x, y), ref Array);
            }
        }
        Mesh nmesh = new Mesh();
        nmesh.vertices = Array.Verts;
        nmesh.normals = Array.normals;
        nmesh.triangles = Array.tri;
        if (WeldVerts)
        {
            var welder = new MeshWelder(nmesh);
            welder.Weld();
        }
        Instance.mesh = nmesh;
        return nmesh;
    }

    //Generate a grid of points for the mesh to use.
    void BuildInitalPlane()
    {
        if (Instance == null)
        {
            Start();
        }
        if (!Handle.IsCompleted)
        {
            Handle.Complete();
        }
        MeshGenTimer.Begin();
        mesh = BuildOrGetMesh();
        mesh.MarkDynamic();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        TargetMeshFilter.mesh = mesh;
        SetUpJob();
        TriangleCount = mesh.triangles.Length;
        VertexCount = mesh.vertices.Length;
        MeshGenTimer.End();

        Debug.Log("Mesh generation took " + MeshGenTimer.GetMS() + "MS ");
    }

    void SetUpJob()
    {
        if (vertices.IsCreated)
        {
            vertices.Dispose();
            PreTransfromedVerts.Dispose();
#if USE_CULLING
            CullFlags.Dispose();
#endif
        }
        vertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
        PreTransfromedVerts = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
#if USE_CULLING
        CullFlags = new NativeArray<int>(vertices.Length, Allocator.Persistent);
#endif
        RenderJob = new FluidRenderJob();
        RenderJob.vertices = vertices;
#if USE_CULLING
        RenderJob.Culled = CullFlags;
#endif
        IFData.RenderType = FluidToRender;
        RenderJob.InstanceData = IFData;
#if USE_CULLING
        CullingJob = new FluidCullJob();
        CullingJob.Culled = CullFlags;
        CullingJob.vertices = vertices;
        CullingJob.InstanceData = IFData;
#endif
        UpdateTransform();
    }

    void UpdateTransform()
    {
        THREAD_transfrom = transform;
        for (int i = 0; i < vertices.Length; i++)
        {
            PreTransfromedVerts[i] = transform.TransformPoint(vertices[i]);
        }
        RenderJob.TransformedVerts = PreTransfromedVerts;
    }
#if USE_CULLING
    public static bool ShouldTileCulled(Vector3 pos, InstancedFluidData IFD)
    {
        if (Instance.DisableCulling)
        {
            return false;
        }
        Tile t = Instance.TileMap.FindCell(pos);
        if (t != null)
        {
            if (t.CurrentFluidType != IFD.RenderType && t.CurrentFluidType != Fluid.FluidType.None)
            {
                return true;
            }

            if (t.IsCellSettled())
            {
                return true;
            }

            if (t.GetFluidLevel() > 0.0f)
            {
                return false;
            }

            //check near cell
            if (t.HasBuilding() || t.IsBlocked())
            {
                return false;
            }
        }
        return true;
    }
#endif
    float GetFluidAtTile(int x, int y, InstancedFluidData IFD)
    {
        Tile t = TileMap.GetTileAtPos(x, y);
        if (t == null)
        {
            return 0.0f;
        }
        if (t.CurrentFluidType != IFD.RenderType)
        {
            return 0.0f;
        }
        return t.GetFluidRenderingHeight();
    }

    Vector3 ClampToTile(Tile t, Vector3 pos, InstancedFluidData IFD, bool Origin = false)
    {
        float HalfTileSize = Origin ? IFD.OriginClamp : IFD.SourceClamp;
        pos.x = Mathf.Clamp(pos.x, t.Thread_Cached_WorldPos.x - HalfTileSize, t.Thread_Cached_WorldPos.x + HalfTileSize);
        pos.z = Mathf.Clamp(pos.z, t.Thread_Cached_WorldPos.z - HalfTileSize, t.Thread_Cached_WorldPos.z + HalfTileSize);
        return pos;
    }

    public static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0;
        b.y = 0;
        return Vector3.Distance(a, b);
    }

    bool SmoothToTile(int x, int y, ref Vector3 pos, InstancedFluidData IFD, Tile origin, bool Diagonal)
    {
#if DEBUG_RENDER
        if (DisableSmoothTO)
        {
            return false;
        }
#endif
        Tile t = TileMap.GetTileAtPos(x, y);
        if (t == null)
        {
            return false;
        }
        if (t.CurrentFluidType != IFD.RenderType && t.CurrentFluidType != Fluid.FluidType.None)
        {
            pos.y = -FluidBuildingZeroOffset;
            return false;
        }
        float fluidlevel = t.GetFluidRenderingHeight();

        if (fluidlevel > FluidEngine.MIN_SIM_VALUE)
        {
            pos.y = fluidlevel;
            //  if (origin.IsSpecialFluidBlock())
            {
                pos = ClampToTile(origin, pos, IFD);
            }
            return true;
        }
        return false;
    }

    bool StrechToTile(int x, int y, InstancedFluidData IFD, Vector3 pos, out Vector3 outpos)
    {
        outpos = pos;
        Tile t = TileMap.GetTileAtPos(x, y);
        if (t == null)
        {
            return false;
        }
        if (t.HasBuilding() || t.IsBlocked())
        {
            outpos = ClampToTile(t, pos, IFD, true);
            return true;
        }
        return false;
    }

    float GetNearFluidLevel(int x, int y, InstancedFluidData IFD)
    {
        float avg = GetFluidAtTile(x - 1, y, IFD);
        avg = Mathf.Max(avg, GetFluidAtTile(x, y + 1, IFD));
        avg = Mathf.Max(avg, GetFluidAtTile(x + 1, y, IFD));
        avg = Mathf.Max(avg, GetFluidAtTile(x, y - 1, IFD));
        return avg;
    }

    bool IsNextToBuilding(int x, int y, Vector3 pos, InstancedFluidData IFD)
    {
        for (int i = 0; i < SimpleDirectionsLength; i++)
        {
            Tile t = TileMap.GetTileAtPos(x + Directions[i].x, y + Directions[i].y);
            if (t != null)
            {
                if (t.HasBuilding() || t.IsBlocked())
                {
                    float dist = FlatDistance(pos, t.Thread_Cached_WorldPos);
                    if (dist < IFD.SoftSkinDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    Vector3 GetWeightedHeight(Vector3 inputpos, bool WorldSpace, InstancedFluidData IFD, out bool building)
    {
        building = false;
        Vector3 OutputPos = inputpos;
        if (!WorldSpace)
        {
            inputpos = THREAD_transfrom.TransformPoint(inputpos);
        }
        inputpos.y = 0.0f;
        if (DisableSkinning)
        {
            OutputPos.y = -FluidZeroOffset;
            return OutputPos;
        }
        Tile t = TileMap.FindCell(inputpos);
        if (t == null)
        {
            OutputPos.y = -FluidZeroOffset;
            return OutputPos;
        }
        bool IsOtherFluid = (t.CurrentFluidType != IFD.RenderType && t.CurrentFluidType != Fluid.FluidType.None);

        OutputPos.y = t.GetFluidRenderingHeight();
        if (t.IsBlocked() || t.HasBuilding() || IsOtherFluid)
        {
            building = true;
            for (int i = 0; i < Directions.Length; i++)
            {
#if DEBUG_RENDER
                if (DisableDiagonals)
                {
                    if (i > 3)
                    {
                        break;
                    }
                }
#endif
                if (IsOtherFluid)
                {
                    if (i > 3)
                    {
                        break;
                    }
                }
                if (SmoothToTile(t.X + Directions[i].x, t.Y + Directions[i].y, ref OutputPos, IFD, t, i > 3))
                {
                    return OutputPos;
                }
            }
        }
        else
        {
            if (IsNextToBuilding(t.X, t.Y, OutputPos, IFD))
            {
                building = true;//flatten down!
            }
        }
        if (IsOtherFluid)
        {
            OutputPos.y = -FluidZeroOffset;
            //if (t.GetFluidLevel() < 0.1f && t.CurrentFluidType == Fluid.FluidType.Lava)
            //{
            //    OutputPos.y = -MixSkinHeight;
            //}
            //else
            //{
            //    OutputPos.y = -FluidBuildingZeroOffset;
            //}

        }
        return OutputPos;
    }

    public static void ComputePoint(ref Vector3 pos, Vector3 Worldpos, InstancedFluidData IFD)
    {
        bool IsBuilding = false;
        pos = Instance.GetWeightedHeight(Worldpos, true, IFD, out IsBuilding);
        if (pos.y <= FluidEngine.MIN_SIM_VALUE)
        {
            pos.y = IsBuilding ? -Instance.FluidBuildingZeroOffset : -Instance.FluidZeroOffset;
        }
    }

    private void CopyToMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.MarkDynamic();
        if (FluidToRender != Fluid.FluidType.Lava)//lava needs no normals!
        {
            if (Time.frameCount % UpdateNormalsEveryFrames == 0)
            {
                mesh.RecalculateNormals();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mesh == null)
        {
            return;
        }
        DisableSkinning = WorldController.GetMode() == WorldController.CurrentGameMode.Editor;
        if (!ShouldUpdate())
        {
            if (CellCountLast == 0)
            {
                return;
            }
        }
#if USE_CULLING
        if (UseCullingJob)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0.0f)
            {
                currentTime = CullingPeriod;
                JobHandle CullHandle = CullingJob.Schedule(vertices.Length, BatchCount);
                CullHandle.Complete();
                if (CullingStats)
                {
                    CalulateStats();
                }
            }
        }
#endif
        CellCountLast = Engine.TilesWater;
        if (UseJobSystem)
        {
#if UNITY_EDITOR
            RenderJob.InstanceData = IFData;
#endif
            if (RefreshTransfrom)
            {
                UpdateTransform();
                RefreshTransfrom = false;
            }
            Handle = RenderJob.Schedule(vertices.Length, BatchCount);
        }
        else
        {
            IFData.RenderType = FluidToRender;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = vertices[i];
                ComputePoint(ref p, PreTransfromedVerts[i], IFData);
                vertices[i] = p;
            }
        }
        Handle.Complete();
        CopyToMesh();
        ForceUpdate = false;
    }

    public override void OnGridCreationFinished()
    {
        BuildInitalPlane();
        ForceUpdate = true;
    }

    bool ShouldUpdate()
    {
        if (ForceUpdate)
        {
            return true;
        }
        if (FluidToRender == Fluid.FluidType.Lava)
        {
            return (Engine.TilesLava > 0);
        }
        else if (FluidToRender == Fluid.FluidType.Water)
        {
            return (Engine.TilesWater > 0);
        }
        return false;
    }
    void CalulateStats()
    {
#if USE_CULLING
        int Culled = 0;
        foreach (int i in CullFlags)
        {
            if (i == 1)
            {
                Culled++;
            }
        }
        CulledPC = ((float)Culled / (float)vertices.Length) * 100;
#endif
    }

    public override void OnGridDestruction()
    {
        mesh = null;
    }
}

struct FluidRenderJob : IJobParallelFor
{
    public NativeArray<Vector3> vertices;
    [ReadOnly]
    public NativeArray<Vector3> TransformedVerts;
    [ReadOnly]
    public FluidRenderer.InstancedFluidData InstanceData;
#if USE_CULLING
    [ReadOnly]
    public NativeArray<int> Culled;
#endif
    public void Execute(int i)
    {
#if USE_CULLING
        if (Culled[i] == 1)
        {
            return;
        }
#endif
        Vector3 p = vertices[i];
        FluidRenderer.ComputePoint(ref p, TransformedVerts[i], InstanceData);
        vertices[i] = p;
    }
}

#if USE_CULLING
struct FluidCullJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3> vertices;
    public NativeArray<int> Culled;
    [ReadOnly]
    public FluidRenderer.InstancedFluidData InstanceData;
    public void Execute(int i)
    {
        Culled[i] = FluidRenderer.ShouldTileCulled(vertices[i], InstanceData) ? 1 : 0;
    }
}
#endif