using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level3Objectives : ObjectivesContoler
{
    Objective _objective;
    [SerializeField]
    int CrystalCount = 20;
    // Start is called before the first frame update
    protected override void AfterStart()
    {
        _objective = new Objective("Collect " + CrystalCount.ToString() + " Energy Crystals", true, CrystalCount);
        _objectiveMenu.ObjectivesList.Add(_objective);
        _objectiveMenu.UpdateObjectiveList();
    }

    // Update is called once per frame
    protected override void WithUpdate()
    {
        if (_objective.CurrentProgress != _worldController._energyCrystalsCount)
        {
            _objective.UpdateProgress(_worldController._energyCrystalsCount);
        }
    }

    protected override bool WinCondtions()
    {
        return _objective.Complete;
    }

    protected override bool LoseCondtions()
    {
        if (StanderLossCondtions())
        {
            return true;
        }
        if (_worldController._energyCrystalsOnMap + _worldController._energyCrystals.Count + _worldController._energyCrystalsCount < CrystalCount)
        {
            _defeatReason = "Ran out of Energy Crystals to reach your goal";
            return true;
        }
        return false;
    }

}
