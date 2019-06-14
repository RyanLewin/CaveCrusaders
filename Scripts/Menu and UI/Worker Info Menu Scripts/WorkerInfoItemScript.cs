using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInfoItemScript : MonoBehaviour
{
    [SerializeField]
    Text _nameText, _taskText;
    Worker _worker;

    void Update()
    {
        if (gameObject.activeInHierarchy && _worker != null)
        {
            _taskText.text = (_worker.GetCurrentTask() != null ? _worker.GetCurrentTask()._taskDescription : "");
        }
    }

    public void SetInfo(Worker worker)
    {
        if (worker == null)
        {
            return;
        }
        _worker = worker;
        _nameText.text = worker._name;
        _taskText.text = (worker.GetCurrentTask() != null ? worker.GetCurrentTask()._taskDescription : "");
    }
}
