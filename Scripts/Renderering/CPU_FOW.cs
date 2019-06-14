using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

public class CPU_FOW : MonoBehaviour
{
    [Header("Config")]
    [SerializeField]
    float UpdateRate = 0.25f;
    [Header("Setup")]
    [SerializeField]
    FogOfWarDecalController FOW;
    [Header("Debug")]
    [SerializeField]
    bool DebugActive = false;
    [SerializeField]
    bool DEBUG_fowBlack = false;
    [SerializeField]
    float DebugSize = 100;
    [SerializeField]
    float DebugStep = 1.0f;
    Texture2D CPUTex;
    AsyncGPUReadbackRequest Request;
    bool RequestInFlight = false;
    RenderTexture TargetRT;
    float CurrentcoolDown = 0.0f;
    TileMap3D Map = null;
    bool IsCPUTexValid = false;

    bool Active = true;
    Vector2Int[] Directions;
    [SerializeField]
    bool DebugRequests = false;
    public float TextureRes = 1;
    DecalProjectorComponent Decal;
    Vector3 RootPos;
    public static CPU_FOW Get()
    {
        return WorldController.GetWorldController.CPUFOW;
    }
    public Vector3 GetRootPos()
    {
        return RootPos;
    }
    // Start is called before the first frame update
    void Start()
    {
        Map = WorldController.GetWorldController.GetComponent<TileMap3D>();
        Decal = FOW.GetComponentInChildren<DecalProjectorComponent>();
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

    public void SetTexture(RenderTexture RT)
    {
        if (Decal == null)//ensure start
        {
            Start();
        }
        TargetRT = RT;
        RequestInFlight = false;
        CreateTexture(RT);
        IsCPUTexValid = false;
        RootPos = transform.position + new Vector3(Decal.m_Size.x / 2, 0, Decal.m_Size.z / 2);
    }

    void CreateTexture(RenderTexture RT)
    {
        CPUTex = new Texture2D(RT.width, RT.height, TextureFormat.R8, false);//todo:
    }

    void DebugS()
    {
        if (!DebugActive)
        {
            return;
        }
        Vector3 StartPos = transform.position - new Vector3(Decal.m_Size.x / 2, 0, -Decal.m_Size.z / 2);
        // Debug.DrawLine(RootPos, RootPos + Vector3.up * 150, Color.white);

        for (int i = 0; i < DebugSize; i++)
        {
            for (int y = 0; y < DebugSize; y++)
            {
                Vector3 pos = StartPos + new Vector3((i * DebugStep), 0, -(y * DebugStep));
                bool FOW = SampleFOW(pos);
                Debug.DrawLine(pos, pos + Vector3.up * 20, FOW ? Color.blue : Color.red);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active)
        {
            return;
        }
        DebugS();
        CurrentcoolDown -= Time.deltaTime;
        if (TargetRT == null)
        {
            return;
        }
        if (Request.done)
        {
            UpdateCPUTex();
        }
        if (CurrentcoolDown > 0.0f)
        {
            return;
        }
        CurrentcoolDown = UpdateRate;
        RequestTexture();
    }

    void RequestTexture()
    {
        RequestInFlight = true;
        Request = AsyncGPUReadback.Request(TargetRT);
    }

    void UpdateCPUTex()
    {
        if (!RequestInFlight)
        {
            return;
        }
        if (Request.hasError)
        {
            return;
        }
        NativeArray<int> array = Request.GetData<int>();
        CPUTex.LoadRawTextureData(array);
        CPUTex.Apply();
        RequestInFlight = false;
        IsCPUTexValid = true;
    }
    bool ByPassFOW()
    {
        return (CPUTex == null || !IsCPUTexValid || !isActiveAndEnabled || !FOW.EnableCloseFog);
    }
    public bool MultiSampleFOW(Vector3 pos, float Distance)
    {
        if (ByPassFOW())
        {
            return true;//allow the UI though if we ant valid
        }
        for (int i = 0; i < Directions.Length; i++)
        {
            Vector3 newpos = pos + new Vector3(Directions[i].x * Distance, 0, Directions[i].y * Distance);
            if (SampleFOW(newpos))
            {
                return true;
            }

        }
        return false;
    }

    public bool SampleFOW(Vector3 pos)
    {
        if (DEBUG_fowBlack)
        {
            return false;
        }
        if (ByPassFOW())
        {
            return true;//allow the UI though if we ant valid
        }
        Vector3 PosDS = pos - RootPos;
        float TextureSpaceX = PosDS.x * TextureRes;
        float TextureSpaceY = PosDS.z * TextureRes;
        Vector2Int TextureSamplePos = new Vector2Int(Mathf.RoundToInt(TextureSpaceX), Mathf.RoundToInt(TextureSpaceY));
        Color Pixel = CPUTex.GetPixel(TextureSamplePos.x, TextureSamplePos.y);
        bool State = Pixel.r > 0.0f;
        if (DebugRequests)
        {
            Debug.DrawLine(pos, pos + Vector3.up * 20, State ? Color.green : Color.red, 30);
        }
        return State;
    }
}
