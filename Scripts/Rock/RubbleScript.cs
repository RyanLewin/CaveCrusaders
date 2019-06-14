using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleScript : Tile
{
    [SerializeField] List<AudioClip> _energyCrystalAudio = new List<AudioClip>();

    [SerializeField] Material _dirt;
    [SerializeField] Material _looseRock;
    [SerializeField] Material _hardRock;

    [SerializeField] GameObject _rubble;
    [SerializeField] float _brakeTime = 3;
    [SerializeField] GameObject _dropedObject;
    [SerializeField] int _maxDropedObject;
    [SerializeField] int _minDropedObject;

    [SerializeField] bool _energyCrystal;


   // public AudioClip crumble;
    [Header("Debug")]
    [SerializeField] bool _beingDug;

    WorldController _worldController;
    SoundManager _soundManager;
    int _objectAmount;
    float _stateSwichTime;
    float _stateSwichWait;
    RockScript.Type _rockType;

    public RockScript.Type RockType { get { return _rockType; } }
    public bool EnergyCrystal { get { return _energyCrystal; } }

    // Start is called before the first frame update
    void Start()
    {
        _worldController = WorldController.GetWorldController;
        _soundManager = _worldController._soundManager;
        if (_rubble.GetComponentInChildren<MeshRenderer>().material.name.Contains("Dirt"))
        {
            _rockType = RockScript.Type.Dirt;
        }
        else if (_rubble.GetComponentInChildren<MeshRenderer>().material.name.Contains("LooseRock"))
        {
            _rockType = RockScript.Type.LooseRock;
        }

        else if (_rubble.GetComponentInChildren<MeshRenderer>().material.name.Contains("SolidRock"))
        {
            _rockType = RockScript.Type.HardRock;
        }



        UnitTask tempTask = new UnitTask
        {
            _location = _rubble.transform.position,
            _taskType = UnitTask.TaskType.ClearRubble,
            _targetRubble = this,
            _requiredTool = Unit.UnitTool.Shovel,
            _taskDescription = "Clearing rubble"
        };
        TaskList.AddTaskToGlobalTaskList(tempTask);


        if (_energyCrystal)
        {
            if (_rockType == RockScript.Type.Dirt)
            {

                _objectAmount = 1;

            }
            else if (_rockType == RockScript.Type.LooseRock)
            {
                _objectAmount = 2;

            }
            else if (_rockType == RockScript.Type.HardRock)
            {

                _objectAmount = 3;

            }
            _stateSwichWait = _brakeTime / _objectAmount;

            _stateSwichTime = _brakeTime - _stateSwichWait;
        }
        else
        {
            if ((_objectAmount = Random.Range(_minDropedObject, _maxDropedObject)) != 0)
            {
                _stateSwichWait = _brakeTime / _objectAmount;

                _stateSwichTime = _brakeTime - _stateSwichWait;
            }
        }
    }

    // Update is called once per frame
    protected override void TileUpdate()
    {
        if (_beingDug)
        {
            Dig();
        }
    }

    public override int GetID()
    {
        return (int)TileTypeID.RubbleTile;
    }

    public void Dig() //DAM YOU!
    {

        if (WorldController._worldController.DEBUG_INSTANT_MINE)
        {
            _brakeTime = 0;
        }
        _brakeTime -= Time.deltaTime;
        if (_brakeTime <= 0)
        {
            if (WorldController._worldController.DEBUG_INSTANT_MINE)
            {
                for (int i = 1; i < _objectAmount; i++)
                {
                    GameObject spawnedObject = (GameObject)Instantiate(_dropedObject, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), new Quaternion(0, 0, 0, 0));
                }
                if (_energyCrystal)
                {

                    _soundManager.PlayAnnouncement(_energyCrystalAudio[Random.Range(0, _energyCrystalAudio.Count)]);
                   
                }
            }

            if (_objectAmount != 0)
            {
                GameObject spawnedObject = (GameObject)Instantiate(_dropedObject, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), new Quaternion(0, 0, 0, 0));
                if (_energyCrystal)
                {

                    _soundManager.PlayAnnouncement(_energyCrystalAudio[Random.Range(0, _energyCrystalAudio.Count)]);
                    
                }
            }
            StaticMapInfo.Map.UpdateTile(x, y, StaticMapInfo.RockModleHolder.DefultTile);

        }
        else if (_brakeTime <= _stateSwichTime)
        {
            GameObject spawnedObject = (GameObject)Instantiate(_dropedObject, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), new Quaternion(0, 0, 0, 0));

            _stateSwichTime -= _stateSwichWait;
        }

    }

    public void ChangeTexture(RockScript.Type rockType)
    {
        if (rockType == RockScript.Type.Dirt)
        {
            _rubble.GetComponentInChildren<MeshRenderer>().material = _dirt;
        }
        else if (rockType == RockScript.Type.LooseRock)
        {
            _rubble.GetComponentInChildren<MeshRenderer>().material = _looseRock;
        }
        else if (rockType == RockScript.Type.HardRock)
        {
            _rubble.GetComponentInChildren<MeshRenderer>().material = _hardRock;
        }
    }


}
