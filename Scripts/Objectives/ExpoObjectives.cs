using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpoObjectives : ObjectivesContoler
{
    // Start is called before the first frame update
    [SerializeField] GameObject _mushroomCluster;
    [SerializeField] GameObject _slug;
    [SerializeField] GameObject _bug;
    [SerializeField] GameObject _ambush;
    Tile _lavaRockTile;
    Tile _waterRockTile;


    int _allTogetherMuschrooms;
    Objective _mushroomObjective;
    Objective _vichicalObjective;
    //Objective _lavaObjective;
    //Objective _waterObjecive;

    protected override void AfterStart()
    {
        //_lavaRockTile = StaticMapInfo.Map.GetTileAtPos(5, 16);
        //_waterRockTile = StaticMapInfo.Map.GetTileAtPos(13, 24);
        _worldController._mushroomSpawn = false;
        Tile chosenTile = StaticMapInfo.Map.GetTileAtPos(15, 7);

        Vector3 newLocation = chosenTile.transform.position;
        GameObject newCluster = Instantiate(_mushroomCluster, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));

        if (newCluster != null)
        {
            newCluster.GetComponent<MushroomCluster>().FirstMushRoom(newLocation.x, newLocation.y, newLocation.z);
        }

        Instantiate(_ambush, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));
        Instantiate(_bug, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));
        for (int i = 0; i < 3; i++)
        {
            Instantiate(_slug, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));
        }
        _worldController.GrowSpeed = 0.04f;
        _mushroomObjective = new Objective("Burn All Mushroom Clusters", true,0);
        _objectiveMenu.ObjectivesList.Add(_mushroomObjective);
        _vichicalObjective = new Objective("Build a Mining Vehicle", true, 1);
        _objectiveMenu.ObjectivesList.Add(_vichicalObjective);
        //_lavaObjective = new Objective("Find Lava", true, 1);
        //_objectiveMenu.ObjectivesList.Add(_lavaObjective);
        //_waterObjecive = new Objective("Find Water", true, 1);
        //_objectiveMenu.ObjectivesList.Add(_waterObjecive);

        _objectiveMenu.UpdateObjectiveList();

        
    }

    protected override bool WinCondtions()
    {
      
            return (_vichicalObjective.Complete && _mushroomObjective.Complete);
       
    }

    protected override void WithUpdate()
    {
        if (!_mushroomObjective.Complete)
        {
            if (_worldController._mushroomClusters.Count == 0)
            {
                _mushroomObjective.ProgressGoal = 1;
                _mushroomObjective.UpdateProgress(1);
                _objectiveMenu.UpdateObjectiveList();
            }
        }

        //if (!_lavaObjective.Complete)
        //{
        //    if (_lavaRockTile == null)
        //    {
        //        _lavaObjective.UpdateProgress(1);
        //        _objectiveMenu.UpdateObjectiveList();
        //    }
        //}
        //if (!_waterObjecive.Complete)
        //{
        //    if (_waterRockTile == null)
        //    {
        //        _waterObjecive.UpdateProgress(1);
        //        _objectiveMenu.UpdateObjectiveList();
        //    }
        //}
       if (!_vichicalObjective.Complete)
        {
            foreach (Unit unit in _worldController._workers)
            {
                if (unit.GetComponent<Vehicle>() != null)
                {
                    _vichicalObjective.UpdateProgress(1);
                    _objectiveMenu.UpdateObjectiveList();
                }
            }
        }
    }
    }
