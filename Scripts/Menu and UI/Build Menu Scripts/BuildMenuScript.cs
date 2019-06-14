using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuScript : MonoBehaviour
{
    public static BuildMenuScript instance;

    [SerializeField]
    GameObject _buildContentView, _costDisplay;
    [SerializeField]
    Text _descriptionText;
    
    public Dictionary<string, bool> _allowBuild = new Dictionary<string, bool>()
    {
        { "Skip", true },
        { "OxyGen", true },
        { "Blockade", true },
        { "Garage", true },
        { "Outpost", true },
        { "H2OConvertor", true },
        { "Turret", true },
        { "PowerGen", true },
    };

    List<string> _buildingDescriptions = new List<string>()
    {
        "SKIP \nChuck resources in here",
        "OXY-GEN \nBalloon pump for units",
        "LAVA BLOCKADE \nLava is bad! Stop lava",
        "GARAGE \nVroom vroom",
        "OUTPOST \nNeed more hands?",
        "H2O CONVERTOR \nNeed water, I mean oxygen..",
        "TURRET \nPew pew!",
        "POWER-GEN \nUnlimited power! (terms and conditions apply)",
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        ClearToolTip();
    }

    /// <summary>
    /// Set all build menu buttons interactable if the building can be afforded
    /// </summary>
    public void UpdateBuildMenu()
    {
        for (int i = 0; i < _buildContentView.transform.childCount; i++)
        {
            _buildContentView.transform.GetChild(i).GetComponent<BuildMenuItemScript>().SetBuildButtonInteractive();
        }
    }

    /// <summary>
    /// Hide all cancel buttons in the build menu after finishing building placement
    /// </summary>
    public void FinishBuild()
    {
        for (int i = 0; i < _buildContentView.transform.childCount; i++)
        {
            _buildContentView.transform.GetChild(i).GetComponent<BuildMenuItemScript>().HideCancel();
        }
    }

    public void ShowBuildingToolTip(int buildingIndex)
    {
        _costDisplay.SetActive(true);
        _descriptionText.text = _buildingDescriptions[buildingIndex];
        _buildContentView.transform.GetChild(buildingIndex).GetComponent<BuildMenuItemScript>().SetBuildingCostText();
    }

    public void ClearToolTip()
    {
        _costDisplay.SetActive(false);
        _descriptionText.text = "";
    }

    public bool GetAllowBuild(int buildIndex)
    {
        switch (buildIndex)
        {
            case 0:
                return _allowBuild["Skip"];
            case 1:
                return _allowBuild["OxyGen"];
            case 2:
                return _allowBuild["Blockade"];
            case 3:
                return _allowBuild["Garage"] && WorldController.GetWorldController._miningLevel >= WorldController.MiningLevel.two;
            case 4:
                return _allowBuild["Outpost"];
            case 5:
                return _allowBuild["H2OConvertor"];
            case 6:
                return _allowBuild["Turret"];
            case 7:
                return _allowBuild["PowerGen"];
        }

        return false;
    }
}
