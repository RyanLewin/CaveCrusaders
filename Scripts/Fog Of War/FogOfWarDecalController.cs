using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering.PostProcessing;



public class FogOfWarDecalController : IGridNotify
{
    [Header("Setup")]
    [SerializeField]
    Material DecalMat = null;
    [SerializeField]
    Material WaterMat = null;
    [SerializeField]
    Camera DarkCam = null;
    [SerializeField]
    Camera LightCam = null;
    [SerializeField]
    DecalProjectorComponent DecalPro;

    [Header("Resolution")]
    [SerializeField]
    float DarkPixelsPerTile = 10;
    [SerializeField]
    float LightPixelsPerTile = 50;
    [ReadOnly, SerializeField]
    int DarkRTSize = 128;
    [ReadOnly, SerializeField]
    int LightRTSize = 1024;
    [SerializeField]
    bool MSAA = true;//should the render targets use Multi sample anti-aliasing

    [Header("Size")]
    [SerializeField]
    TileMap3D Map = null;
    [SerializeField]
    public float GridSizeRatio = 1.0f;//ratio between tiles and camera projection.
    [ReadOnly, SerializeField]
    int CameraOrthoSize = 100;//the size of the cameras orthographic projection 
    [Header("Debug")]
    public bool Clear = false;//flag set to clear the RTs in the next update
    [SerializeField]
    public bool EnableCloseFog = false;//toggle the decal on and off.
    CPU_FOW CPUFOW = null;//FOW on the CPU
    RenderTexture DarkRT = null;
    RenderTexture LightRT = null;

    public int GetDarkRTSize()
    {
        return DarkRTSize;
    }

    public float GetDarkPixelsPerTile()
    {
        return DarkPixelsPerTile;
    }

    // Start is called before the first frame update
    void Start()
    {
        //init all RTs and update the sizes 
        CPUFOW = WorldController._worldController.GetComponentInChildren<CPU_FOW>();
        DecalMat = new Material(DecalMat);//instance the material 
        DecalPro.Mat = DecalMat;
        ClearRT(ref DarkRT, true);
        ClearRT(ref LightRT, false);
        UpdateCamSizes();
    }
    private void OnDestroy()
    {
        foreach (Material m in FOWMats)
        {
            ClearMaterial(m);
        }
        if (WaterMat != null)
        {
            //Update water mat with the state of FOW
            WaterMat.SetInt("Boolean_675E54A4", 0);
            WaterMat.SetFloat("_LightRTSize", 0);
            WaterMat.SetFloat("_DarkRTSize", 0);
            if (CPUFOW != null)
            {
                WaterMat.SetVector("Vector3_69E84236", Vector4.zero);
            }
        }
        if (DecalPro != null)
        {
            DecalPro.Mat.SetFloat("_FOWBlend", 0.0f);
            DecalPro.Mat.SetFloat("_DecalBlend", 0.0f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Clear)
        {
            //clear was requested so: update and clear   
            UpdateCamSizes();
            ClearRT(ref DarkRT, true);
            ClearRT(ref LightRT, false);
            UpdateCamSizes();
            Clear = false;
        }
    }
    Vector4 Shader_Rootpos;
    /// <summary>
    /// Update the camera sizes and center the entire FOW blob on the map
    /// also calculates the size of RTs based on the grid size
    /// </summary>
    public void UpdateCamSizes()
    {
        if (Map != null)
        {
            //using the Pixels Per Tile values calculate the render targets size
            CameraOrthoSize = Mathf.RoundToInt(Mathf.Max(Map.GridXSize_, Map.GridYSize_) * GridSizeRatio);
            DarkRTSize = Mathf.RoundToInt(Mathf.Max(Map.GridXSize_, Map.GridYSize_) * DarkPixelsPerTile);
            LightRTSize = Mathf.RoundToInt(Mathf.Max(Map.GridXSize_, Map.GridYSize_) * LightPixelsPerTile);
        }
        Vector3 newpos = Map.GetGridCentre();
        newpos.y = transform.position.y;
        transform.position = newpos;//center to the map
        DarkCam.orthographicSize = CameraOrthoSize;//set the cameras orthographic Size.
        LightCam.orthographicSize = CameraOrthoSize;//set the cameras orthographic Size.
        //camera orthographic Size is doubled as the orthographic is distance from camera center to edge of a square.
        DecalPro.m_Size = new Vector3(CameraOrthoSize * 2, 200, CameraOrthoSize * 2);
        DecalPro.OnEnable();//force the decal to update
        if (DecalPro != null)
        {
            //update fields that enable and disable the FOW
            DecalPro.Mat.SetFloat("_FOWBlend", EnableCloseFog ? 1.0f : 0.0f);
            DecalPro.Mat.SetFloat("_DecalBlend", EnableCloseFog ? 1.0f : 0.0f);
        }
        if (WaterMat != null)
        {
            //Update water mat with the state of FOW
            WaterMat.SetInt("Boolean_675E54A4", EnableCloseFog ? 1 : 0);
            WaterMat.SetFloat("_LightRTSize", GetDarkRTSize());
            WaterMat.SetFloat("_DarkRTSize", GetDarkRTSize());
            if (CPUFOW != null)
            {
                WaterMat.SetVector("Vector3_69E84236", new Vector4(CPUFOW.GetRootPos().x, CPUFOW.GetRootPos().y, CPUFOW.GetRootPos().z, 0));
            }
        }
        HandleLinkedMaterials();
    }
    /// <summary>
    /// Create or Recreate the Render targets
    /// </summary>
    /// <param name="RT">Target RT</param>
    /// <param name="IsDark">Does the RT belong to the Dark shroud</param>
    void ClearRT(ref RenderTexture RT, bool IsDark)
    {
        if (RT != null)
        {
            RT.Release();//enqueue the old texture for release.
        }

        int size = IsDark ? DarkRTSize : LightRTSize;
        RenderTextureDescriptor desc = new RenderTextureDescriptor(size, size);//create a new texture description
        desc.msaaSamples = MSAA ? 8 : 1;//do we want some smoothnesses?
        desc.colorFormat = RenderTextureFormat.R8;//configure format of the RT todo: pick at smaller format like R8?
        RT = new RenderTexture(desc);//create a new texture with the desc we have.
        RT.filterMode = FilterMode.Trilinear;//best filter a render texture can do :( (I want anisotropic filtering!)
        RT.wrapMode = TextureWrapMode.Repeat;
        if (IsDark)
        {
            DarkCam.targetTexture = RT;//tell the dark cam to write to this texture, instead of the back buffer
            DecalMat.SetTexture("_FOWMap", RT);//Update the decal projectors material with the dark FOW map
            if (WaterMat != null)
            {
                WaterMat.SetTexture("Texture2D_F2FB17FF", RT);//update the water with the dark FOW map (aren't generated names great!)
            }
            if (CPUFOW != null)
            {
                CPUFOW.SetTexture(RT);//update the CPU side FOW
            }
        }
        else
        {
            if (WaterMat != null)
            {
                WaterMat.SetTexture("Texture2D_7DED7969", RT);//update the water with the light FOW map (aren't generated names great!)
            }
            DecalMat.SetTexture("_BaseColorMap", RT);//Update the decal projectors material with the light FOW map
            LightCam.targetTexture = RT;//tell the dark cam to write to this texture, instead of the back buffer
        }
    }

    public override void OnGridCreationFinished()
    {
        //we have a new map possibly: clear everything and resize.        
        Clear = true;
        UpdateCamSizes();
    }
    [SerializeField]
    Material[] FOWMats;
    void HandleLinkedMaterials()
    {
        foreach (Material m in FOWMats)
        {
            UpdateMaterialWithRT(m);
        }
    }
    void UpdateMaterialWithRT(Material M)
    {
        M.SetFloat("_USEFOW", EnableCloseFog ? 1.0f : 0.0f);
        M.SetTexture("_FOWLight", LightRT);
        M.SetTexture("_FOWDark", DarkRT);
        if (CPUFOW != null)
        {
            M.SetVector("_FOWRootPos", new Vector4(CPUFOW.GetRootPos().x, CPUFOW.GetRootPos().y, CPUFOW.GetRootPos().z, 0));
        }
        M.SetVector("_FOWSizes", new Vector4(DarkRTSize, LightRTSize, 0, 0));
    }
    void ClearMaterial(Material M)
    {
        M.SetFloat("_USEFOW", 0.0f);
        M.SetTexture("_FOWLight", null);
        M.SetTexture("_FOWDark", null);
        if (CPUFOW != null)
        {
            M.SetVector("_FOWRootPos", Vector4.zero);
        }
        M.SetVector("_FOWSizes", Vector4.zero);
    }
}
