using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    public static SettingScript instance;

    [SerializeField]
    Toggle _mousePanToggle, _mouseZoomToggle;
    [SerializeField]
    Slider _panSpeedSlider, _zoomSpeedSlider, _riskLevelSlider;
    [SerializeField]
    Dropdown _unitPriorityDropDown;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //Initialise();

        _mousePanToggle.isOn = MousePanEnabled;
        _mouseZoomToggle.isOn = MouseZoomEnabled;
        _panSpeedSlider.value = CameraPanSpeed;
        _zoomSpeedSlider.value = CameraZoomSpeed;
        _riskLevelSlider.value = RiskLevel;
        _unitPriorityDropDown.value = UnitPriority;
    }

    ///// <summary>
    ///// Initialise settings using playerprefs or defaults
    ///// </summary>
    //public void Initialise()
    //{
    //    _mousePanEnabled = PlayerPrefs.GetInt("Mouse Pan Enabled", 1) == 1? true : false;
    //    _mouseZoomEnabled = PlayerPrefs.GetInt("Mouse Zoom Enabled", 1) == 1? true : false;
    //    _cameraPanSpeed = PlayerPrefs.GetFloat("Camera Pan Speed", 20f);
    //    _cameraZoomSpeed = PlayerPrefs.GetFloat("Camera Zoom Speed", 1.5f);
    //    _riskLevel = PlayerPrefs.GetInt("Risk Level", 20);
    //    _unitPriority = PlayerPrefs.GetInt("Unit Priority", 0);
    //    _cameraRotSpeed = PlayerPrefs.GetFloat("CameraRotSpeed", 2);
    //}
       
    public bool MousePanEnabled
    {
        get => SettingManager.singleton._mousePanEnabled;
        set
        {
            SettingManager.singleton._mousePanEnabled = value;
            PlayerPrefs.SetInt("Mouse Pan Enabled", value == true? 1 : 0);
        }
    }

    public bool MouseZoomEnabled
    {
        get => SettingManager.singleton._mouseZoomEnabled;
        set
        {
            SettingManager.singleton._mouseZoomEnabled = value;
            PlayerPrefs.SetInt("Mouse Zoom Enabled", value == true? 1 : 0);
        }
    }

    public float CameraPanSpeed
    {
        get => SettingManager.singleton._cameraPanSpeed;
        set
        {
            SettingManager.singleton._cameraPanSpeed = value;
            PlayerPrefs.SetFloat("Camera Pan Speed", value);
        }
    }
    public float CameraRotSpeed
    {
        get => SettingManager.singleton._cameraRotSpeed;
        set
        {
            SettingManager.singleton._cameraRotSpeed = value;
            PlayerPrefs.SetFloat("CameraRotSpeed", value);
        }
    }
    public float CameraZoomSpeed
    {
        get => SettingManager.singleton._cameraZoomSpeed;
        set
        {
            SettingManager.singleton._cameraZoomSpeed = value;
            PlayerPrefs.SetFloat("Camera Zoom Speed", value);
        }
    }

    public float RiskLevel
    {
        get => SettingManager.singleton._riskLevelValue;
        set
        {
            SettingManager.singleton._riskLevelValue = (int)value;
            PlayerPrefs.SetInt("Risk Level", (int)value);
        }
    }

    public int UnitPriority
    {
        get => SettingManager.singleton._unitPriority;
        set
        {
            SettingManager.singleton._unitPriority = value;
            PlayerPrefs.SetInt("Unit Priority", value);
        }
    }
}