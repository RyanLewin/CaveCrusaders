using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : MonoBehaviour
{
    float _timmer = 0;
    bool _added = false;
    [SerializeField] AudioClip _floorHit;
    [SerializeField] AudioSource _audioSource;
    // Start is called before the first frame update
    void Start()
    {
        
        
    }
    private void Update()
    {
        //check if its fallen through floor
        if(transform.position.y < -1)
        {
            transform.position = new Vector3 (transform.position.x,5,transform.position.z);
        }


        if (_timmer > 2 && !_added)
        {
            UnitTask tempTask = new UnitTask
            {
                _location = transform.position,
                _taskType = UnitTask.TaskType.Pickup,
                _taskDescription = "Transporting an Ore",
                _itemToPickUp = transform.gameObject,
                _itemType = UnitTask.ItemType.Ore,
                _requiredTool = Unit.UnitTool.none
            };
            if (SettingScript.instance.UnitPriority == 0)
            {
                TaskList.AddTaskToGlobalTaskList(tempTask);
            }
            else
            {
                TaskList.Tasks.Insert(0, tempTask);
            }
            _added = true;
        }
        else
        {
            _timmer += Time.deltaTime;
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        _audioSource.clip = _floorHit;
        _audioSource.Play();
       // WorldController.GetWorldController._soundManager.PlaySFX(floorHit);
    }
}
