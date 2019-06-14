using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterViewing : MonoBehaviour
{
    List<int> _monsterVisable = new List<int>();
    WorldController _worldController;
    [SerializeField] FogOfWarDecalController _fogOfWarDecalController;
    [SerializeField]
    float UpdateRate = 0.1f;
    float currentUpdate = 0.0f;
    bool _oldBattleMode = false;
    // Start is called before the first frame update
    void Start()
    {
        _worldController = WorldController._worldController;
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdate -= Time.deltaTime;
        if (currentUpdate > 0.0f)
        {
            _worldController._soundManager.BattleMode = _oldBattleMode;
            return;
        }
        currentUpdate = UpdateRate;
        if (WorldController.IsPlaying())
        {
            _monsterVisable.Clear();
        }
        foreach (Unit worker in _worldController._workers)
        {
            MonsterVeiw(worker.transform.position, Mathf.Max(worker.FogRange.transform.localScale.x / 2, worker.FogRange.transform.localScale.z / 2));
        }
        foreach (Building building in _worldController._buildings)
        {
            MonsterVeiw(building.transform.position, Mathf.Max(building.FogRange.transform.localScale.x / 2, building.FogRange.transform.localScale.z / 2));
        }

        bool battleMode = false;
        for (int n = 0; n < _worldController._monsters.Count; n++)
        {
            Monster thisMonster = _worldController._monsters[n];
            bool visable = _monsterVisable.Contains(n);
            if (_fogOfWarDecalController != null)
            {
                if (!_fogOfWarDecalController.EnableCloseFog)
                {
                    visable = true;
                }
            }
            thisMonster.SetVisiblity(visable);
            if (!battleMode)
            {
                battleMode = visable;
            }
        }
        
        _worldController._soundManager.BattleMode = battleMode;
        _oldBattleMode = battleMode;


    }
    /// <summary>
    /// Makes the monster visable if someone is close enough
    /// </summary>
    /// <param name="viewer"> The location of the GameObject that intracts with the Monstor </param>    
    void MonsterVeiw(Vector3 viewer, float radius)
    {
        // Revel monster if viewer is close
        int monsterNo = 0;

        foreach (Monster monster in WorldController._worldController._monsters)
        {
            if (Vector3.Distance(monster.transform.position, viewer) < radius)
            {
                _monsterVisable.Add(monsterNo);
            }
            monsterNo++;
        }
    }
}

