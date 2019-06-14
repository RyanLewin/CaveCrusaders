using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlScript : MonoBehaviour
{
    public static ControlScript instance;

    bool _waitingForKey;
    KeyCode _newKey;
    public List<Control> ControlList { get; set; }

    [SerializeField]
    GameObject _controlsContentView, _controlContentItemPrefab, _cancelButton, _waitingText;
    public const string ORDERBRUSH_MINE = "Mine Paint";
    public const string ORDERBRUSH_CANCEL = "Cancel Paint";
    public const string LEFTCONTROL = "Snap Camera";
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Initialise();

        _waitingForKey = false;
    }

    /// <summary>
    /// Initialise control list using player prefs or defaults
    /// </summary>
    public void Initialise()
    {
        Debug.Log("init");
        ControlList = new List<Control>
        {
            //Order controls
            new Control("Select", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Select1", "Mouse0")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Select2", "None")) }, new KeyCode[]{ KeyCode.Mouse0, KeyCode.None }),
            new Control("Order", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Order1", "Mouse1")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Order2", "None")) }, new KeyCode[]{ KeyCode.Mouse1, KeyCode.None }),
            new Control("Order Modifier", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Order Modifier1", "LeftShift")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Order Modifier2", "None")) }, new KeyCode[]{ KeyCode.LeftShift, KeyCode.None }),
            
            //Camera controls
            new Control("Camera Forward", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Forward1", "W")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Forward2", "UpArrow")) }, new KeyCode[]{ KeyCode.W, KeyCode.UpArrow }),
            new Control("Camera Backward", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Backward1", "S")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Backward2", "DownArrow")) }, new KeyCode[]{ KeyCode.S, KeyCode.DownArrow }),
            new Control("Camera Left", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Left1", "A")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Left2", "LeftArrow")) }, new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow }),
            new Control("Camera Right", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Right1", "D")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Right2", "RightArrow")) }, new KeyCode[]{ KeyCode.D, KeyCode.RightArrow }),
            new Control("Camera Zoom In", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Zoom In1", "Minus")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Zoom In2", "Home")) }, new KeyCode[]{ KeyCode.Minus, KeyCode.Home }),
            new Control("Camera Zoom Out", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Zoom Out1", "Plus")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Zoom Out2", "End")) }, new KeyCode[]{ KeyCode.Plus, KeyCode.End }),
            new Control("Camera Rotate", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Rotate1", "Mouse2")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Rotate2", "Delete")) }, new KeyCode[]{ KeyCode.Mouse2, KeyCode.Delete }),
            new Control("Camera Snap Left", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Snap Left1", "Q")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Snap Left2", "None")) }, new KeyCode[]{ KeyCode.Q, KeyCode.None }),
            new Control("Camera Snap Right", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Snap Right1", "E")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Camera Snap Right2", "None")) }, new KeyCode[]{ KeyCode.E, KeyCode.None }),
            new Control("Toggle Camera Movement", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Toggle Camera Movement1", "L")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Toggle Camera Movement2", "None")) }, new KeyCode[]{KeyCode.L, KeyCode.None }),
            new Control(LEFTCONTROL, new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LEFTCONTROL1", "LeftControl")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LEFTCONTROL2", "RightControl")) }, new KeyCode[]{ KeyCode.LeftControl, KeyCode.RightControl }),
           

            //Hotkeys
            new Control("Pause", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Pause1", "Escape")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Pause2", "None")) }, new KeyCode[]{ KeyCode.Escape, KeyCode.None }),
            new Control("Panic", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Panic1", "P")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Panic2", "None")) }, new KeyCode[]{ KeyCode.P, KeyCode.None }),
            new Control("Build Menu", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Build Menu1", "B")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Build Menu2", "None")) }, new KeyCode[]{ KeyCode.B, KeyCode.None }),
            new Control("Tasks Menu", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Tasks Menu1", "T")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Tasks Menu2", "None")) }, new KeyCode[]{ KeyCode.T, KeyCode.None }),
            new Control("Objectives Menu", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Objectives Menu1", "O")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Objectives Menu2", "None")) }, new KeyCode[]{ KeyCode.O, KeyCode.None }),
            new Control("Minimap", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Minimap1", "M")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Minimap2", "None")) }, new KeyCode[]{ KeyCode.M, KeyCode.None }),
            new Control("Grid Menu", new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Grid1", "G")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Grid2", "None")) }, new KeyCode[]{ KeyCode.G, KeyCode.None }),

            new Control(ORDERBRUSH_MINE, new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ORDERBRUSH_MINE1", "R")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ORDERBRUSH_MINE2", "None")) }, new KeyCode[]{ KeyCode.R, KeyCode.None }),
            new Control(ORDERBRUSH_CANCEL, new KeyCode[]{ (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ORDERBRUSH_CANCEL1", "C")), (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ORDERBRUSH_CANCEL2", "None")) }, new KeyCode[]{ KeyCode.C, KeyCode.None })
        };

        for (int i = 0; i < ControlList.Count; i++)
        {
            GameObject controlEditorItem = Instantiate(_controlContentItemPrefab);
            controlEditorItem.transform.SetParent(_controlsContentView.transform);

            for (int j = 0; j < controlEditorItem.GetComponentsInChildren<Button>().Length; j++)
            {
                controlEditorItem.GetComponentsInChildren<Button>()[j].GetComponentInChildren<Text>().text = ControlList[i].Bindings[j].ToString();
                controlEditorItem.GetComponentsInChildren<Button>()[j].onClick.AddListener(() => StartAssignment());
            }
            controlEditorItem.GetComponentInChildren<Text>().text = ControlList[i].Name;
            controlEditorItem.name = ControlList[i].Name;
        }
    }

    void OnGUI()
    {
        if (Event.current.isKey && _waitingForKey)
        {
            _newKey = Event.current.keyCode;
            CancelAssignment();
        }
        else if (Event.current.isMouse && _waitingForKey)
        {
            _newKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Mouse" + Event.current.button);
            CancelAssignment();
        }
    }

    /// <summary>
    /// Restore all controls to their default keys and update the controls button label
    /// </summary>
    public void RestoreDefaults()
    {
        for (int i = 0; i < ControlList.Count; i++)
        {
            ControlList[i].RestoreDefaultBindings();
            for (int j = 0; j < _controlsContentView.transform.GetChild(i).GetComponentsInChildren<Button>().Length; j++)
            {
                _controlsContentView.transform.GetChild(i).GetComponentsInChildren<Button>()[j].GetComponentInChildren<Text>().text = ControlList[i].Bindings[j].ToString();
                PlayerPrefs.SetString(ControlList[i].Name + (j + 1), ControlList[i].Bindings[j].ToString());
            }
        }
    }

    /// <summary>
    /// Cancel assign key process
    /// </summary>
    public void CancelAssignment()
    {
        _waitingForKey = false;
        _waitingText.SetActive(false);
        _cancelButton.SetActive(false);
    }

    /// <summary>
    /// Start assign key coroutine passing in the button game object
    /// </summary>
    public void StartAssignment()
    {
        if (!_waitingForKey)
        {
            _waitingText.SetActive(true);
            _cancelButton.SetActive(true);
            StartCoroutine(AssignKey(EventSystem.current.currentSelectedGameObject));
        }
    }

    /// <summary>
    /// Assign the control keycode and update the button text after key press
    /// </summary>
    /// <param name="controlButtonGameObject">Button game object that was clicked</param>
    public IEnumerator AssignKey(GameObject controlButtonGameObject)
    {
        _waitingForKey = true;
        yield return new WaitUntil(() => Input.anyKey || _waitingForKey == false);
        
        if (_newKey != KeyCode.None)
        {
            ControlList[controlButtonGameObject.transform.parent.GetSiblingIndex()].Bindings[controlButtonGameObject.transform.GetSiblingIndex() - 1] = _newKey;
            PlayerPrefs.SetString(ControlList[controlButtonGameObject.transform.parent.GetSiblingIndex()].Name + controlButtonGameObject.transform.GetSiblingIndex(), _newKey.ToString());
            controlButtonGameObject.GetComponentInChildren<Text>().text = _newKey.ToString();
            _waitingForKey = false;
        }
        EventSystem.current.SetSelectedGameObject(null);
        yield break;
    }

    public Control GetControl(string name)
    {
        return ControlList.Find(control => control.Name == name);
    }
}

public class Control
{
    KeyCode[] _defaultBindings;
    public string Name { get; }
    public KeyCode[] Bindings { get; set; }

    public Control(string name, KeyCode[] bindings, KeyCode[] defaultBindings)
    {
        Name = name;
        Bindings = bindings;
        _defaultBindings = defaultBindings;
    }

    public void RestoreDefaultBindings()
    {
        Bindings = _defaultBindings;
    }

    public bool AnyInput
    {
        get
        {
            foreach (KeyCode binding in Bindings)
            {
                if (Input.GetKey(binding))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool InputDown
    {
        get
        {
            foreach (KeyCode binding in Bindings)
            {
                if (Input.GetKeyDown(binding))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool InputUp
    {
        get
        {
            foreach (KeyCode binding in Bindings)
            {
                if (Input.GetKeyUp(binding))
                {
                    return true;
                }
            }
            return false;
        }
    }

    string GetFriendlyKeyName(KeyCode keyCode)
    {
        switch (keyCode)
        {
            //case Mouse0:
            //    return "Left Mouse Button";
            //case Mouse1:
            //    return "Right Mouse Button";
            //case Mouse2:
            //    return "Middle Mouse Button";
            default:
                return "";
        }
    }
}
