using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Reinforecments : MonoBehaviour
{
    [SerializeField] bool _Build;
    bool _Built = false;
    [SerializeField] List<VisualEffect> smokeVFXList = new List<VisualEffect>();
    public Worker _Builder;
    [SerializeField] float _endY;

    [SerializeField] MeshRenderer _north;
    [SerializeField] MeshRenderer _south;
    [SerializeField] MeshRenderer _east;
    [SerializeField] MeshRenderer _west;

    float _BuildProgress = 0;
    float _TimeToBuild = 5;

    [SerializeField] Building building;

    public bool Built { get { return _Built; } }

    // Start is called before the first frame update
    void Start()
    {
        UnitTask tempTask = new UnitTask
        {
            _location = transform.position,
            _taskType = UnitTask.TaskType.Build,
            _targetBuilding = building,
            _requiredTool = Unit.UnitTool.Hammer,
            _taskDescription = "Build a building"
        };
        TaskList.AddTaskToGlobalTaskList(tempTask);
        foreach (VisualEffect smokeVFX in smokeVFXList)
        {
            if (smokeVFX)
            {
                smokeVFX.Stop();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Build();
    }

    public void Build()
    {
        if ((_Builder && !_Built )|| (_Build && !_Built))
        {
            foreach (VisualEffect smokeVFX in smokeVFXList)
            {
                if (smokeVFX)
                {
                    smokeVFX.Play();
                }
            }
            _BuildProgress += Time.deltaTime;
           transform.position = new Vector3(transform.position.x, Mathf.Min(_endY - 5f + _BuildProgress, _endY), transform.position.z);

            if (_BuildProgress >= _TimeToBuild)
            {
                _Built = true;
                foreach (VisualEffect smokeVFX in smokeVFXList)
                {
                    if (smokeVFX)
                    {
                        smokeVFX.Stop();
                    }
                }
            }
        }
    }

    public void SetWalls(bool north,bool east,bool south,bool west)
    {
        _north.enabled = north;
        _east.enabled = east;
        _south.enabled = south;
        _west.enabled = west;
    }


}
