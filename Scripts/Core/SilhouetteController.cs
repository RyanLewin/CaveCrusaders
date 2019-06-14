using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class SilhouetteController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField]
    OutLineMode CurrnetMode = OutLineMode.Quality;
    [SerializeField]
    float DepthRatio = 0.5f;
    [Header("Setup")]

    [SerializeField]
    GameObject SilhouteeleGO = null;
    Camera SilhouetteCam = null;
    [SerializeField]
    PostProcessVolume MainCamVolume = null;
    [SerializeField]
    PostProcessVolume SilhouetteCamVolume = null;
    OutlineBliter BLiter = null;
    [SerializeField]
    GameObject DepthCamGO = null;

    RenderTexture Tex = null;
    Camera DepthCam = null;//this is sooooo stupid - are you sure now using a depth only pass also consider frame cyclic dependencies?
    RenderTexture DepthBuffer = null;
    Bloom bloom;
    DepthFilter Filter = null;
    public enum OutLineMode { Off, Simple, Quality_NoBloom, Quality, Limit };
    [Header("Debug")]
    [SerializeField]
    public bool ShowDepth = false;

    public bool ForceClear = false;
    public bool ShowRT = false;
    // Start is called before the first frame update
    void Start()
    {
        SilhouetteCam = SilhouteeleGO.GetComponent<Camera>();
        DepthCam = DepthCamGO.GetComponent<Camera>();

        MainCamVolume.sharedProfile.TryGetSettings(out BLiter);
        SilhouetteCamVolume.profile.TryGetSettings(out Filter);
        SilhouetteCamVolume.profile.TryGetSettings(out bloom);
        BLiter.enabled.value = true;
        BLiter.active = true;
        UpdateMode();
    }
    public void SetMode(OutLineMode Mode)
    {
        CurrnetMode = Mode;
        UpdateMode();
    }
    public OutLineMode GetMode()
    {
        return CurrnetMode;
    }
    void UpdateMode()
    {
        switch (CurrnetMode)
        {
            case OutLineMode.Off:
                SilhouteeleGO.SetActive(false);
                DepthCamGO.SetActive(false);
                BLiter.enabled.value = false;
                SilhouetteCamVolume.enabled = false;
                break;
            case OutLineMode.Simple:
                SilhouteeleGO.SetActive(true);
                BLiter.enabled.value = true;
                Filter.enabled.value = false;
                SilhouetteCamVolume.enabled = false;
                break;
            case OutLineMode.Quality:
                DepthCamGO.SetActive(true);
                SilhouteeleGO.SetActive(true);
                BLiter.enabled.value = true;
                Filter.enabled.value = true;
                SilhouetteCamVolume.enabled = true;
                if (bloom != null)
                {
                    bloom.enabled.value = true;
                }
                break;
            case OutLineMode.Quality_NoBloom:
                DepthCamGO.SetActive(true);
                SilhouteeleGO.SetActive(true);
                BLiter.enabled.value = true;
                Filter.enabled.value = true;
                SilhouetteCamVolume.enabled = true;
                if (bloom != null)
                {
                    bloom.enabled.value = false;
                }
                break;
        }
        BLiter.Debug.overrideState = true;
        Filter.Debug.overrideState = true;
        Filter.EnableFilter.overrideState = true;
        Filter.EnableFilter.value = 1;
        if (ShowDepth)
        {
            BLiter.Debug.value = 1;
            Filter.Debug.value = 1;
            Filter.EnableFilter.value = 0;
        }
        else
        {
            BLiter.Debug.value = ShowRT ? 1 : 0;
            Filter.Debug.value = 0;
            Filter.EnableFilter.value = ShowRT ? 1 : 0;
        }

    }

    private void UpdateRT(ref RenderTexture RT, bool ISDepth)
    {
        int Width = Screen.width;
        int Height = Screen.height;
        if (ISDepth)
        {
            Width = Mathf.RoundToInt(Screen.width * DepthRatio);
            Height = Mathf.RoundToInt(Screen.height * DepthRatio);
        }
        if (RT != null && RT.width == Width && RT.height == Height && !ForceClear)
        {
            return;
        }
        if (RT != null)
        {
            RT.Release();
        }
        RenderTextureDescriptor desc = new RenderTextureDescriptor(Width, Height);
        desc.msaaSamples = 1;
        desc.colorFormat = RenderTextureFormat.ARGBHalf;
        if (ISDepth)
        {
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.depthBufferBits = 16;
        }
        RT = new RenderTexture(desc);
        RT.filterMode = FilterMode.Trilinear;
        RT.Create();
        if (ISDepth)
        {
            DepthCam.targetTexture = RT;
            if (Filter != null)
            {
                Filter.DepthB.value = RT;
            }
        }
        else
        {
            SilhouetteCam.targetTexture = RT;
            // somewhere during initializing
            if (BLiter != null)
            {
                BLiter.RT.value = RT;
            }
            if (Filter != null)
            {
                Filter.RT.value = RT;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRT(ref Tex, false);
        UpdateRT(ref DepthBuffer, true);
        ForceClear = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UpdateMode();
#endif
    }
}
