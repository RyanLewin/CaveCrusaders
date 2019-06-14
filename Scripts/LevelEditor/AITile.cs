using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITile : Tile
{
    [SerializeField] List<GameObject> _aI;

    // Start is called before the first frame update
    protected override void OnTileStart()
    {
        WorldController worldController = WorldController.GetWorldController;
        if (WorldController.IsPlaying())
        {
            foreach (GameObject aI in _aI)
            {
                aI.transform.parent = null;


                if (aI.tag == "Worker")
                {
                    worldController._workers.Add(aI.GetComponent<Unit>());
                    worldController._levelStatsController.UnitSpawned();
                }
                if (aI.tag == "Monster")
                {
                    worldController._monsters.Add(aI.GetComponent<Monster>());
                }
            }
            StaticMapInfo.Map.UpdateTile(x, y, StaticMapInfo.RockModleHolder.DefultTile);
        }
        else
        {
            foreach (GameObject aI in _aI)
            {
                Worker worker = aI.GetComponent<Worker>();
                if (worker != null)
                {
                    worker.SetWorkersStatus(false);
                }
                Monster monster = aI.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.SetMonsterStatus(false);
                }
            }
        }
    }
    private void Start()
    {
        if (!WorldController.IsPlaying())
        {
            foreach (GameObject aI in _aI)
            {
                Worker worker = aI.GetComponent<Worker>();
                if (worker != null)
                {
                    worker.SetWorkersStatus(false);
                }
                Monster monster = aI.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.SetMonsterStatus(false);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        //StaticMapInfo.Map.UpdateTile(x, y, StaticMapInfo.RockModleHolder.DefultTile);
    }
}
