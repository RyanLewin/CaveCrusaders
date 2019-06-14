using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField]
    GameObject SelectionObject = null;
    public bool _selected;
    Worker _W;
    public void SetSelectionState(bool state)
    {
        _selected = state;
        if (SelectionObject != null)
        {
            SelectionObject.SetActive(state);
        }
    }

    void Start()
    {
        SetSelectionState(false);
        WorldController.GetWorldController.MouseSelectionControl.RegisterSelectable(this);
        _W = GetComponent<Worker>();
        enabled = (_W != null);
    }
    private void OnDestroy()
    {
        SetSelectionState(false);
        WorldController.GetWorldController.MouseSelectionControl.UnRegisterSelectable(this);
    }
    public bool IsSelectable()
    {
        if (_W != null)
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }
            return !_W.Dead && !_W._inVehicle;
        }
        return true;
    }
    void Update()
    {
        if (!IsSelectable())
        {
            Destroy(this);
        }
    }
}
