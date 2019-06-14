using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System;

public class GraphicScript : MonoBehaviour
{
    [SerializeField]
    Toggle _fullscreenToggle, _vsyncToggle, _anisotropicFilteringToggle, _ambientOcclusionToggle, _bloomToggle, _vignettingToggle, _frameRateLimitToggle;
    [SerializeField]
    Slider _frameRateLimitSlider, _brightnessSlider, _shadowDistanceSlider;
    [SerializeField]
    Dropdown _resolutionDropDown, _presetDropDown, _antialiasDropDown, _textureQualityDropDown;

    void Start()
    {
        SettingManager.singleton.IsLoading = true;
        VSyncEnabled = SettingManager.singleton.FromInt(PlayerPrefs.GetInt(nameof(VSyncEnabled), 0));
        Brightness = PlayerPrefs.GetFloat(nameof(Brightness), 1);
        SettingManager.singleton.IsLoading = false;
        _presetDropDown.value = PlayerPrefs.GetInt("QualityPreset");
        _fullscreenToggle.isOn = FullScreenEnabled;
        _resolutionDropDown.value = ScreenResolution;
        _antialiasDropDown.value = AntialiasingMode;
        _textureQualityDropDown.value = TextureQuality;
        _brightnessSlider.value = Brightness;
        _shadowDistanceSlider.value = ShadowDistance;
        _vsyncToggle.isOn = VSyncEnabled;
        _anisotropicFilteringToggle.isOn = AnisotropicFilteringEnabled;
        _ambientOcclusionToggle.isOn = AmbientOcclusionEnabled;
        _bloomToggle.isOn = BloomEnabled;
        _vignettingToggle.isOn = VignetteEnabled;
        _frameRateLimitToggle.isOn = LimitFrameRate;
        _frameRateLimitSlider.value = SettingManager.singleton.IN_FPSLimit;
        GenerateResolutionList();
    }

    void GenerateResolutionList()
    {
        //if (SettingManager.singleton.GeneratedLength == Screen.resolutions.Length)
        //{
        //    return;
        //}
        _resolutionDropDown.ClearOptions();
        List<string> resolutionStrings = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            resolutionStrings.Add(Screen.resolutions[i].ToString());
        }
        SettingManager.singleton.GeneratedLength = Screen.resolutions.Length;
        _resolutionDropDown.AddOptions(resolutionStrings);
    }
    //saved by unity
    public bool FullScreenEnabled
    {
        get => Screen.fullScreen;
        set
        {
            Screen.fullScreen = value;
            if (!value)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, value);
            }
        }
    }

    public int ScreenResolution
    {
        get
        {
            GenerateResolutionList();
            return SettingManager.singleton.GetCurrentResIndex();
        }
        set => SettingManager.singleton.SetResolution(Screen.resolutions[value]);
    }
    //used when a valid PP is not present
    int TmpAAMode = 0;
    public int AntialiasingMode
    {
        get => SettingManager.singleton.AntialiasingMode;
        set => SettingManager.singleton.AntialiasingMode = value;
    }

    public int TextureQuality
    {
        get => SettingManager.singleton.TextureQuality;
        set => SettingManager.singleton.TextureQuality = value;
    }
    
    public bool LimitFrameRate
    {
        get => SettingManager.singleton.LimitFrameRate;
        set => SettingManager.singleton.LimitFrameRate = value;
    }

    public float FrameRateLimit
    {
        get => SettingManager.singleton.FrameRateLimit;
        set => SettingManager.singleton.FrameRateLimit = value;
    }
    //not part of a preset!
    public bool VSyncEnabled
    {
        get => SettingManager.singleton.VSyncEnabled;
        set => SettingManager.singleton.VSyncEnabled = value;
    }
    //not part of a preset!
    public float Brightness
    {
        get => SettingManager.singleton.Brightness;
        set => SettingManager.singleton.Brightness = value;
    }

    public float ShadowDistance
    {
        get => SettingManager.singleton.ShadowDistance;
        set => SettingManager.singleton.ShadowDistance = value;
    }

    public bool AnisotropicFilteringEnabled
    {
        get => SettingManager.singleton.AnisotropicFilteringEnabled;
        set => SettingManager.singleton.AnisotropicFilteringEnabled = value;
    }

    public bool AmbientOcclusionEnabled
    {
        get => SettingManager.singleton.AmbientOcclusionEnabled;
        set => SettingManager.singleton.AmbientOcclusionEnabled = value;
    }

    public bool BloomEnabled
    {
        get => SettingManager.singleton.BloomEnabled;
        set => SettingManager.singleton.BloomEnabled = value;
    }

    public bool VignetteEnabled
    {
        get => SettingManager.singleton.VignetteEnabled;
        set => SettingManager.singleton.VignetteEnabled = value;
    }
}
