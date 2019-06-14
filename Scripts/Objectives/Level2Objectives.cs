using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Objectives : ObjectivesContoler
{
    Objective _objective;

    // Start is called before the first frame update
    protected override void AfterStart()
    {
        WorldController.GetWorldController.GetComponentInChildren<Fluid>()._FlowRate = 0.02f;
        _objective = new Objective("Collect 10 Energy Crystals", true, 10);
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
        if (_worldController._energyCrystalsOnMap + _worldController._energyCrystals.Count + _worldController._energyCrystalsCount < 10)
        {
            _defeatReason = "Ran out of Energy Crystals to reach your goal";
            return true;
        }
        return false;
    }


}

