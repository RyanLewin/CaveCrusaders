using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInfoPanelScript : MonoBehaviour
{
    public static WorkerInfoPanelScript instance;

    [SerializeField]
    Text _nameText, _taskText;
    Worker _worker;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            _taskText.text = (_worker.GetCurrentTask() != null ? _worker.GetCurrentTask()._taskDescription : "");
            gameObject.transform.position = Camera.main.WorldToScreenPoint(_worker.transform.position);
        }
    }

    public void Open(Worker worker)
    {
        _worker = worker;
        _nameText.text = worker._name;
        _taskText.text = (worker.GetCurrentTask() != null ? worker.GetCurrentTask()._taskDescription : "");
        gameObject.transform.position = worker.transform.position;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
