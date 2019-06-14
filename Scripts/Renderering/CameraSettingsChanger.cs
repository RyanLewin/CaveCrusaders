using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class CameraSettingsChanger : MonoBehaviour
{
    [SerializeField]
    FrameSettings NonMainCameraSettings;
    [SerializeField]
    Camera test;
    [SerializeField]
    List<string> MSAACams;
    [SerializeField]
    List<string> PostProcessingCams;
    // Start is called before the first frame update
    void PopulateArray()
    {

        MSAACams.Add("FOWCam");
        PostProcessingCams.Add("FOWCam");
        PostProcessingCams.Add("FOWDarkCam");
        PostProcessingCams.Add("Silhouettle Camera");
       
        NonMainCameraSettings.enableAtmosphericScattering = false;
        NonMainCameraSettings.enableContactShadows = false;
        NonMainCameraSettings.enableDecals = false;
        NonMainCameraSettings.enableDepthPrepassWithDeferredRendering = false;
        NonMainCameraSettings.enableDistortion = false;
        NonMainCameraSettings.enableLightLayers = false;
        NonMainCameraSettings.enableMotionVectors = false;
        NonMainCameraSettings.enableMSAA = false;
        NonMainCameraSettings.enableObjectMotionVectors = false;
        NonMainCameraSettings.enableOpaqueObjects = true;
        NonMainCameraSettings.enablePostprocess = false;
        NonMainCameraSettings.enableRealtimePlanarReflection = false;
        NonMainCameraSettings.enableReprojectionForVolumetrics = false;
        NonMainCameraSettings.enableRoughRefraction = false;
        NonMainCameraSettings.enableShadow = false;
        NonMainCameraSettings.enableShadowMask = false;
        NonMainCameraSettings.enableSSAO = false;
        NonMainCameraSettings.enableSSR = false;
        NonMainCameraSettings.enableSubsurfaceScattering = false;
        NonMainCameraSettings.enableTransmission = false;
        NonMainCameraSettings.enableTransparentObjects = false;
        NonMainCameraSettings.enableTransparentPostpass = false;
        NonMainCameraSettings.enableTransparentPrepass = false;
        NonMainCameraSettings.enableVolumetrics = false;
        NonMainCameraSettings.lightLoopSettings.enableBigTilePrepass = true;
        NonMainCameraSettings.lightLoopSettings.enableComputeLightEvaluation = false;
        NonMainCameraSettings.lightLoopSettings.enableComputeLightVariants = false;
        NonMainCameraSettings.lightLoopSettings.enableFptlForForwardOpaque = false;
        NonMainCameraSettings.lightLoopSettings.enableComputeMaterialVariants = false;
        NonMainCameraSettings.lightLoopSettings.enableTileAndCluster = false;
        NonMainCameraSettings.lightLoopSettings.overrides = (LightLoopSettingsOverrides)~0;
        NonMainCameraSettings.enableAsyncCompute = true;
        NonMainCameraSettings.runContactShadowsAsync = true;
        NonMainCameraSettings.runLightListAsync = true;
        NonMainCameraSettings.runSSAOAsync = true;
        NonMainCameraSettings.runSSRAsync = true;
        NonMainCameraSettings.runVolumeVoxelizationAsync = true;
        NonMainCameraSettings.shaderLitMode = LitShaderMode.Forward;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
    public void Test()
    {
        NonMainCameraSettings.enableDecals = false;
        SetCameraSettings(test, NonMainCameraSettings);
    }
    public void Execute()
    {
        PopulateArray();
        Camera[] cams = FindObjectsOfType<Camera>();
        foreach(Camera c in cams)
        {
            ProcessCam(c);
        }
        MSAACams.Clear();
        PostProcessingCams.Clear();
    }
    void ProcessCam(Camera c)
    {
        if(c.tag == "MainCamera")
        {
            return;
        }
        Debug.Log("Processing " + c.name);
        FrameSettings FS = new FrameSettings();
        NonMainCameraSettings.CopyTo(FS); 
        FS.enableOpaqueObjects = true;
        if (MSAACams.Contains(c.name))
        {
            FS.enableMSAA = true;
        }
        if (PostProcessingCams.Contains(c.name))
        {
            FS.enablePostprocess = true;
        }
        SetCameraSettings(c, FS);
        c.gameObject.SetActive(true);
#if UNITY_EDITOR
        EditorUtility.SetDirty(c);
#endif
    }
    public void SetCameraSettings(Camera c,FrameSettings set)
    {
        HDAdditionalCameraData HD = c.GetComponent<HDAdditionalCameraData>();
        c.renderingPath = RenderingPath.Forward;
        HD.renderingPath = HDAdditionalCameraData.RenderingPath.Custom;
       
#if false
       // HD.m_FrameSettings = set;
        HD.m_FrameSettings.shaderLitMode = LitShaderMode.Forward;
        set.CopyTo(HD.m_FrameSettings);
        //Set all in flags
        HD.m_FrameSettings.overrides = (FrameSettingsOverrides)~0;
#else
        Debug.Log("HDCAMERA_MOD Not defined");
#endif
    }
}
