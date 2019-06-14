using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TaskMenuItemScript : MonoBehaviour,IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    GameObject _placeHolderPrefab;
    GameObject _placeHolderObject = null;

    public UnitTask _thisTask;

    public void CancelTask()
    {
        if (_thisTask._taskType == UnitTask.TaskType.Build)
        {
            _thisTask._targetBuilding.DestroyBuilding();
        }
        else
        {
            TaskList.RemoveTask(_thisTask);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TaskMenuScript.instance.ReorderListEnabled == false)
        {
            eventData.pointerDrag = null;
            return;
        }

        _placeHolderObject = Instantiate(_placeHolderPrefab);
        _placeHolderObject.transform.SetParent(transform.parent);
        _placeHolderObject.transform.SetSiblingIndex(transform.GetSiblingIndex());
        transform.SetParent(transform.parent.parent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 tempPos = transform.position;
        tempPos.y = Input.mousePosition.y;
        transform.position = tempPos;

        int newSiblingIndex = _placeHolderObject.transform.parent.childCount;

        for (int i = 0; i < _placeHolderObject.transform.parent.childCount; i++)
        {
            if (transform.position.y > _placeHolderObject.transform.parent.GetChild(i).position.y)
            {
                newSiblingIndex = i;
                
                if (_placeHolderObject.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        _placeHolderObject.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(_placeHolderObject.transform.parent);
        transform.SetSiblingIndex(_placeHolderObject.transform.GetSiblingIndex());
        Destroy(_placeHolderObject);
    }
}
