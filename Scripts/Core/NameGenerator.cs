using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NameGenerator
{
    static List<string> _workerNames = new List<string>()
    { "Grumpy Dave",
      "Scottish Dave-ette",
      "Irish Dave",
      "Southern Dave",
      "Dave-ette",
      "Irish Dave-ette",
      "The Other Dave-ette",
      "Dave From Bristol",
      "The Other Irish Dave-ette",
      "Southern Dave-ette"
    };

    public static string GetWorkerName()
    {
        if (WorldController.GetWorldController.DEBUGSPAWNGARY)
        {
            return "Gary";
        }
        float theRandom = Random.Range(0, 1000);
        if (theRandom != 500)
        {
            return _workerNames[Random.Range(0, _workerNames.Count)];
        }
        else
        {
            return "Gary";
        }
    }
}
