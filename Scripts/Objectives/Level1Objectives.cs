using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Objectives : ObjectivesContoler
{
    Objective _objective;

    // Start is called before the first frame update
    protected override void AfterStart()
    {

    }

    public void addObjectives()
    {
        _objective = new Objective("Collect 5 Energy Crystals", true, 5);
        _objectiveMenu.ObjectivesList.Add(_objective);
        _objectiveMenu.UpdateObjectiveList();
    }

    // Update is called once per frame
   protected override void WithUpdate()
    {
        if (_objective != null)
        {
            if (_objective.CurrentProgress != _worldController._energyCrystalsCount)
            {
                _objective.UpdateProgress(_worldController._energyCrystalsCount);
            }
        }
    }

    protected override bool WinCondtions()
    {
        if (_objective != null)
        {
            return _objective.Complete;
        }
        return false;
    }

    protected override bool LoseCondtions()
    {
        
        if (StanderLossCondtions())
        {
            return true;
        }
        if(_worldController._energyCrystalsOnMap + _worldController._energyCrystals.Count + _worldController._energyCrystalsCount < 5)
        {
            _defeatReason = "Ran out of Energy Crystals to reach your goal";

            return true;   
        }
        return false;

    }


}
