using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioScript : MonoBehaviour
{
    public static AudioScript instance;

    [SerializeField]
    Toggle _subtitlesToggle;
    [SerializeField]
    Slider _masterVolumeSlider, _musicVolumeSlider, _sfxVolumeSlider, _speechVolumeSlider, _uiVolumeSlider;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        //Initialise();

        _subtitlesToggle.isOn = SubtitlesEnabled;
        _masterVolumeSlider.value = MasterVolume;
        _musicVolumeSlider.value = MusicVolume;
        _sfxVolumeSlider.value = SFXVolume;
        _speechVolumeSlider.value = SpeechVolume;
        _uiVolumeSlider.value = UIVolume;
        SetMinVolume(-50);
    }
    void SetMinVolume(float v)
    {
        _masterVolumeSlider.minValue = v;
        _musicVolumeSlider.minValue = v;
        _sfxVolumeSlider.minValue = v;
        _speechVolumeSlider.minValue = v;
        _uiVolumeSlider.minValue = v;
    }
    //void Start()
    //{
    //    Initialise();
    //}
    ///// <summary>
    ///// Initialise settings using playerprefs or defaults
    ///// </summary>
    //public void Initialise()
    //{
    //    _subtitlesEnabled = PlayerPrefs.GetInt("Subtitles Enabled", 1) == 1 ? true : false;
    //    MasterVolume = PlayerPrefs.GetFloat("Master Volume", 0);
    //    MusicVolume = PlayerPrefs.GetFloat("Music Volume", 0);
    //    SFXVolume = PlayerPrefs.GetFloat("SFX Volume", 0);
    //    SpeechVolume = PlayerPrefs.GetFloat("Speech Volume", 0);
    //    UIVolume = PlayerPrefs.GetFloat("UI Volume", 0);
    //}
    public bool SubtitlesEnabled
    {
        get => SettingManager.singleton._subtitlesEnabled;
        set
        {
            SettingManager.singleton._subtitlesEnabled = value;
            PlayerPrefs.SetInt("Subtitles Enabled", value == true ? 1 : 0);
        }
    }

    public float MasterVolume
    {
        get => SettingManager.singleton._masterVolume;
        set
        {
            SettingManager.singleton._masterVolume = value;
            SettingManager.singleton.SetMixerVolumes();
            PlayerPrefs.SetFloat("Master Volume", value);
            PlayerPrefs.Save();
        }
    }

    public float MusicVolume
    {
        get => SettingManager.singleton._musicVolume;
        set
        {
            SettingManager.singleton._musicVolume = value;
            SettingManager.singleton.SetMixerVolumes();
            PlayerPrefs.SetFloat("Music Volume", value);
            PlayerPrefs.Save();
        }
    }

    public float SFXVolume
    {
        get => SettingManager.singleton._sfxVolume;
        set
        {
            SettingManager.singleton._sfxVolume = value;
            SettingManager.singleton.SetMixerVolumes();
            PlayerPrefs.SetFloat("SFX Volume", value);
            PlayerPrefs.Save();
        }
    }

    public float SpeechVolume
    {
        get => SettingManager.singleton._speechVolume;
        set
        {
            SettingManager.singleton._speechVolume = value;
            SettingManager.singleton.SetMixerVolumes();
            PlayerPrefs.SetFloat("Speech Volume", value);
            PlayerPrefs.Save();
           
        }
    }

    public float UIVolume
    {
        get => SettingManager.singleton._uiVolume;
        set
        {
            SettingManager.singleton._uiVolume = value;
            SettingManager.singleton.SetMixerVolumes();
            PlayerPrefs.SetFloat("UI Volume", value);
            PlayerPrefs.Save();
        }
    }
}
