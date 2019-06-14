using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class OrderVisualizer : MonoBehaviour
{
    [SerializeField]
    DecalProjectorComponent _decal;
    UnitTask LinkedTask = null;
    bool IsSetup = false;
    private void Start()
    {
        if (IsSetup)
        {
            return;
        }
        _decal.enabled = false;
        _decal.Mat = new Material(_decal.Mat);
        IsSetup = true;
    }

    public enum OrderVisualType { Mine, Reinforce, Move, Clear, Build, None }
    OrderVisualType CurrnetType = OrderVisualType.None;
    public static OrderVisualType ConvertTaskToOrderType(UnitTask.TaskType t)
    {
        if (t == UnitTask.TaskType.Mine)
        {
            return OrderVisualType.Mine;
        }
        if (t == UnitTask.TaskType.ClearRubble)
        {
            return OrderVisualType.Clear;
        }
        if (t == UnitTask.TaskType.Build)
        {
            return OrderVisualType.Build;
        }
        return OrderVisualType.None;
    }
    public void UpdateVisuals(UnitTask Task)
    {
        if (!IsSetup)
        {
            Start();
        }
        if (Task == null)
        {
            SetIcon(OrderVisualType.None);
            return;
        }
        if (Task._taskType == UnitTask.TaskType.Build)
        {
            SetupBuildingIcon(Task);
        }
        else
        {
            SetIcon(ConvertTaskToOrderType(Task._taskType));
        }
        LinkedTask = Task;
    }

    void SetupBuildingIcon(UnitTask Task)
    {
        if (Task._targetBuilding == null)
        {
            SetIcon(ConvertTaskToOrderType(Task._taskType));
            return;
        }
        CurrnetType = OrderVisualType.Build;
        _decal.Mat.SetTexture("_BaseColorMap", TileLibrary.Get().GetBuildingTexture((Tile.TileTypeID)Task._targetBuilding.GetID()));
        _decal.enabled = (CurrnetType != OrderVisualType.None);
    }

    void SetIcon(OrderVisualType Type)
    {
        CurrnetType = Type;
        _decal.Mat.SetTexture("_BaseColorMap", TileLibrary.Get().GetOrderIcon(CurrnetType));
        _decal.enabled = (Type != OrderVisualType.None);
    }
}
