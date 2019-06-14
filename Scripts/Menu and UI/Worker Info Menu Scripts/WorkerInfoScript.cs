using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInfoScript : MonoBehaviour
{
    public static WorkerInfoScript instance;

    [SerializeField]
    GameObject _workersContentView, _workerContentItemPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void ClearList()
    {
        if (_workersContentView.transform.childCount > 0)
        {
            for (int i = 0; i < _workersContentView.transform.childCount; i++)
            {
                Destroy(_workersContentView.transform.GetChild(i).gameObject);
            }
        }
    }

    public void UpdateList(List<Worker> selectedWorkers)
    {
        if (selectedWorkers.Count < 1)
        {
            _workersContentView.transform.parent.parent.gameObject.SetActive(false);
            return;
        }

        _workersContentView.transform.parent.parent.gameObject.SetActive(true);
        ClearList();
        for (int i = 0; i < selectedWorkers.Count; i++)
        {
            GameObject workerListItem = Instantiate(_workerContentItemPrefab, _workersContentView.transform);
            workerListItem.GetComponent<WorkerInfoItemScript>().SetInfo(selectedWorkers[i]);
        }
    }
}
