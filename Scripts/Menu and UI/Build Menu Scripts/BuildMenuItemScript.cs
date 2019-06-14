using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildMenuItemScript : MonoBehaviour, IPointerEnterHandler, IScrollHandler
{
    [SerializeField]
    Text _costText;
    [SerializeField]
    Button _buildButton, _cancelButton;
    [SerializeField]
    GameObject buildingPrefab;
    [SerializeField]
    ScrollRect MainScroll;

    int _buildingCost;

    void Start()
    {
        _buildingCost = buildingPrefab.GetComponent<Building>()._BuildCost;
        //SetBuildingCostText();
        SetBuildButtonInteractive();
    }

    /// <summary>
    /// Set the building cost in the menu
    /// </summary>
    public void SetBuildingCostText()
    {
        _costText.text = _buildingCost.ToString();
    }

    /// <summary>
    /// Check if the button should be interactable, based on if the building can be afforded
    /// </summary>
    public void SetBuildButtonInteractive()
    {
        _buildButton.interactable = (WorldController.GetWorldController._oreCount >= _buildingCost) && BuildMenuScript.instance.GetAllowBuild(transform.GetSiblingIndex());
    }

    /// <summary>
    /// Hide the cancel build button
    /// </summary>
    public void HideCancel()
    {
        _cancelButton.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData evd)
    {
        BuildMenuScript.instance.ShowBuildingToolTip(transform.GetSiblingIndex());
    }

    public void OnScroll(PointerEventData data)
    {
        MainScroll.OnScroll(data);
    }
}
