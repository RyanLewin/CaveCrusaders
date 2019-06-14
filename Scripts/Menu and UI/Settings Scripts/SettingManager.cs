using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using System;

public class SettingManager : MonoBehaviour
{
    public static SettingManager singleton;

    //Gameplay
    public bool _mousePanEnabled;
    public bool _mouseZoomEnabled;
    public float _cameraPanSpeed;
    public float _cameraRotSpeed;
    public float _cameraZoomSpeed;
    public int _riskLevelValue;
    public int _unitPriority;

    //Audio
    public AudioMixer mixer;
    public bool _subtitlesEnabled;
    public float _masterVolume;
    public float _musicVolume;
    public float _sfxVolume;
    public float _speechVolume;
    public float _uiVolume;

    //Graphics
    [SerializeField]
    PostProcessProfile ProcessProfile;
    PostProcessLayer ProcessLayer;
    List<PostProcessVolume> ActiveProcessVolumes;

    public AmbientOcclusion ambientOcclusionLayer = null;
    public Bloom bloomLayer = null;
    public ColorGrading colorGradingLayer = null;
    public Vignette vignetteLayer = null;
    public const string RefreshRateKey = "Screen SavedRate";
    public int GeneratedLength = 0;
    public bool IsCustom = false;
    public bool IsLoading = false;

    void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        Initialise();
    }

    private void Start()
    {
        SetMixerVolumes();
    }

    /// <summary>
    /// Initialise settings using playerprefs or defaults
    /// </summary>
    public void Initialise()
    {
        //Gameplay
        _mousePanEnabled = PlayerPrefs.GetInt("Mouse Pan Enabled", 1) == 1 ? true : false;
        _mouseZoomEnabled = PlayerPrefs.GetInt("Mouse Zoom Enabled", 1) == 1 ? true : false;
        _cameraPanSpeed = PlayerPrefs.GetFloat("Camera Pan Speed", 20f);
        _cameraZoomSpeed = PlayerPrefs.GetFloat("Camera Zoom Speed", 1.5f);
        _riskLevelValue = PlayerPrefs.GetInt("Risk Level", 20);
        _unitPriority = PlayerPrefs.GetInt("Unit Priority", 0);
        _cameraRotSpeed = PlayerPrefs.GetFloat("CameraRotSpeed", 2);

        //Audio
        _subtitlesEnabled = PlayerPrefs.GetInt("Subtitles Enabled", 1) == 1 ? true : false;
        _masterVolume = PlayerPrefs.GetFloat("Master Volume", 0);
        _musicVolume = PlayerPrefs.GetFloat("Music Volume", 0);
        _sfxVolume = PlayerPrefs.GetFloat("SFX Volume", 0);
        _speechVolume = PlayerPrefs.GetFloat("Speech Volume", 0);
        _uiVolume = PlayerPrefs.GetFloat("UI Volume", 0);

        //Graphics
        ProcessProfile.TryGetSettings(out ambientOcclusionLayer);
        ProcessProfile.TryGetSettings(out bloomLayer);
        ProcessProfile.TryGetSettings(out colorGradingLayer);
        ProcessProfile.TryGetSettings(out vignetteLayer);
        //GetPP();

        IN_limitFrameRate = Convert.ToBoolean(PlayerPrefs.GetInt("LimitFrameRate", 0));
        IN_FPSLimit = PlayerPrefs.GetInt("FrameRateLimit", 60);

#if UNITY_EDITOR
        //force highest quality in editor.
        SetQualityPreset(3);
#endif
        int SavedRate = PlayerPrefs.GetInt(RefreshRateKey, 999);
        if (Screen.currentResolution.refreshRate != SavedRate)
        {
            Resolution r = Screen.currentResolution;
            r.refreshRate = SavedRate;
            SetResolution(r);
        }
        SetQualityPreset(PlayerPrefs.GetInt("QualityPreset", 2));        
        UpdateFramerateCap();
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Master")
        {
            GetPP();
        }
    }

    void GetPP()
    {
        ProcessLayer = Camera.main.GetComponentInChildren<PostProcessLayer>();
        ActiveProcessVolumes = new List<PostProcessVolume>();
        try
        {
            PostProcessManager.instance.GetActiveVolumes(ProcessLayer, ActiveProcessVolumes);
            UpdateVolumeProfile();
        }
        catch { }
    }
    
    public void SetMixerVolumes()
    {
        mixer.SetFloat("masterVolume", _masterVolume);
        mixer.SetFloat("musicVolume", _musicVolume);
        mixer.SetFloat("sfxVolume", _sfxVolume);
        mixer.SetFloat("dialogueVolume", _speechVolume);
        mixer.SetFloat("UIVolume", _uiVolume);
    }

    public void SetResolution(Resolution r)
    {
        Screen.SetResolution(r.width, r.height, Screen.fullScreen, r.refreshRate);
        PlayerPrefs.SetInt(RefreshRateKey, Screen.currentResolution.refreshRate);
        PlayerPrefs.Save();
    }

    public void UpdateVolumeProfile()
    {
        if (ActiveProcessVolumes != null)
        {
            if (ActiveProcessVolumes.Count > 0)
            {
                ActiveProcessVolumes[0].profile = ProcessProfile;
            }
        }
    }

    public void SetQualityPreset(int presetIndex)
    {
        IsCustom = false;
        switch (presetIndex)
        {
            case 0:
                //low
                AntialiasingMode = 0;
                TextureQuality = 3;
                ShadowDistance = 40;
                AnisotropicFilteringEnabled = false;
                AmbientOcclusionEnabled = false;
                BloomEnabled = false;
                VignetteEnabled = true;
                break;
            case 1:
                //med
                AntialiasingMode = 1;
                TextureQuality = 1;
                ShadowDistance = 60;
                AnisotropicFilteringEnabled = true;
                AmbientOcclusionEnabled = true;
                BloomEnabled = true;
                VignetteEnabled = true;
                break;
            case 2:
                //high
                AntialiasingMode = 2;
                TextureQuality = 0;
                ShadowDistance = 80;
                AnisotropicFilteringEnabled = true;
                AmbientOcclusionEnabled = true;
                BloomEnabled = true;
                VignetteEnabled = true;
                break;
            case 3:
                //ultra
                AntialiasingMode = 2;
                TextureQuality = 0;
                ShadowDistance = 100;
                AnisotropicFilteringEnabled = true;
                AmbientOcclusionEnabled = true;
                BloomEnabled = true;
                VignetteEnabled = true;
                break;
            case 4:
                //custom so load me some values
                LoadAll();
                IsCustom = true;
                break;
            default:
                break;
        }

        PlayerPrefs.SetInt("QualityPreset", presetIndex);
    }
    void UpdateFramerateCap()
    {
        if (IsLoading)
        {
            return;
        }
        PlayerPrefs.SetInt("LimitFrameRate", Convert.ToInt16(LimitFrameRate));
        PlayerPrefs.SetInt("FrameRateLimit", IN_FPSLimit);
        PlayerPrefs.SetInt(nameof(VSyncEnabled), ToInt(VSyncEnabled));
        PlayerPrefs.SetFloat(nameof(Brightness), Brightness);
        PlayerPrefs.Save();
        Application.targetFrameRate = LimitFrameRate ? IN_FPSLimit : -1;
    }

    public int GetCurrentResIndex()
    {
        Resolution c = Screen.currentResolution;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution r = Screen.resolutions[i];

            if (r.height == c.height && c.width == r.width && Utils.Utils.FastApproximately(c.refreshRate, r.refreshRate, 1))
            {
                return i;
            }
        }
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution r = Screen.resolutions[i];
            if (r.height == c.height && c.width == r.width)
            {
                return i;
            }
        }
        Debug.LogError("Find Res failed");
        return Screen.resolutions.Length - 1;//just return the highest
    }
    
    //used when a valid PP is not present
    int TmpAAMode = 0;
    public int AntialiasingMode
    {
        get
        {
            if (ProcessLayer != null)
            {
                return (int)ProcessLayer.antialiasingMode;
            }
            else
            {
                return TmpAAMode;
            }
        }
        set
        {
            if (ProcessLayer != null)
            {
                ProcessLayer.antialiasingMode = (PostProcessLayer.Antialiasing)value;
            }
            else
            {
                TmpAAMode = value;
            }
            OnSettingUpdate();
        }
    }

    public int TextureQuality
    {
        get => QualitySettings.masterTextureLimit;
        set
        {
            QualitySettings.masterTextureLimit = value;
            OnSettingUpdate();
        }
    }

    private bool IN_limitFrameRate = false;
    public bool LimitFrameRate
    {
        get => IN_limitFrameRate;
        set
        {
            IN_limitFrameRate = value;
            UpdateFramerateCap();
        }
    }
    public int IN_FPSLimit = 0;
    public float FrameRateLimit
    {
        get => IN_FPSLimit;
        set
        {
            IN_FPSLimit = (int)value;
            UpdateFramerateCap();
        }
    }
    //not part of a preset!
    public bool VSyncEnabled
    {
        get => QualitySettings.vSyncCount == 0 ? false : true;
        set
        {
            QualitySettings.vSyncCount = (value == true ? 1 : 0);
            UpdateFramerateCap();
        }
    }
    //not part of a preset!
    public float Brightness
    {
        get => colorGradingLayer.postExposure.value;
        set
        {
            colorGradingLayer.postExposure.value = value;
            UpdateVolumeProfile();
            UpdateFramerateCap();
        }
    }

    public float ShadowDistance
    {
        get => QualitySettings.shadowDistance;
        set
        {
            QualitySettings.shadowDistance = value;
            OnSettingUpdate();
        }
    }

    public bool AnisotropicFilteringEnabled
    {
        get => QualitySettings.anisotropicFiltering == AnisotropicFiltering.Disable ? false : true;
        set
        {
            QualitySettings.anisotropicFiltering = (value == true ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable);
            OnSettingUpdate();
        }
    }

    public bool AmbientOcclusionEnabled
    {
        get => ambientOcclusionLayer.enabled;
        set
        {
            ambientOcclusionLayer.enabled.value = value;
            UpdateVolumeProfile();
            OnSettingUpdate();
        }
    }

    public bool BloomEnabled
    {
        get => bloomLayer.enabled;
        set
        {
            bloomLayer.enabled.value = value;
            UpdateVolumeProfile();
            OnSettingUpdate();
        }
    }

    public bool VignetteEnabled
    {
        get => vignetteLayer.enabled;
        set
        {
            vignetteLayer.enabled.value = value;
            UpdateVolumeProfile();
            OnSettingUpdate();
        }
    }
    //called when any setting changes 
    void OnSettingUpdate()
    {
        if (IsCustom)
        {
            SaveAllProps();
        }
    }
    //i should not have to write this function!
    int ToInt(bool b)
    {
        return b ? 1 : 0;
    }
    public bool FromInt(int i)
    {
        return i == 1;
    }
    void SaveAllProps()
    {
        PlayerPrefs.SetFloat(nameof(ShadowDistance), ShadowDistance);
        PlayerPrefs.SetInt(nameof(VignetteEnabled), ToInt(VignetteEnabled));
        PlayerPrefs.SetInt(nameof(BloomEnabled), ToInt(BloomEnabled));
        PlayerPrefs.SetInt(nameof(AmbientOcclusionEnabled), ToInt(AmbientOcclusionEnabled));
        PlayerPrefs.SetInt(nameof(AnisotropicFilteringEnabled), ToInt(AnisotropicFilteringEnabled));

        PlayerPrefs.SetInt(nameof(TextureQuality), TextureQuality);
        PlayerPrefs.SetInt(nameof(AntialiasingMode), AntialiasingMode);
        PlayerPrefs.Save();
    }

    void LoadAll()
    {
        ShadowDistance = PlayerPrefs.GetFloat(nameof(ShadowDistance), 100);
        VignetteEnabled = FromInt(PlayerPrefs.GetInt(nameof(VignetteEnabled), 1));
        BloomEnabled = FromInt(PlayerPrefs.GetInt(nameof(BloomEnabled), 1));
        AmbientOcclusionEnabled = FromInt(PlayerPrefs.GetInt(nameof(AmbientOcclusionEnabled), 1));
        AnisotropicFilteringEnabled = FromInt(PlayerPrefs.GetInt(nameof(AnisotropicFilteringEnabled), 1));

        TextureQuality = PlayerPrefs.GetInt(nameof(TextureQuality), 8);
        AntialiasingMode = PlayerPrefs.GetInt(nameof(AntialiasingMode), 1);
    }

}
