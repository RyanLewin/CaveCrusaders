using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TileMenuItemScript : MonoBehaviour, IPointerEnterHandler, IScrollHandler
{
    [SerializeField]
    Button _cancelButton;
    [SerializeField]
    ScrollRect MainScroll;

    public void OnPointerEnter(PointerEventData evd)
    {
        LevelEditorUIScript.instance.ShowTileToolTip(transform.GetSiblingIndex());
    }

    public void OnScroll(PointerEventData data)
    {
        MainScroll.OnScroll(data);
    }

    /// <summary>
    /// Hide the cancel build button
    /// </summary>
    public void HideCancel()
    {
        _cancelButton.gameObject.SetActive(false);
    }
}
