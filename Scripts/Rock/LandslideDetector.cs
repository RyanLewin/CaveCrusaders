using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandslideDetector : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ore" || other.tag == "EnergyCrystal")
        {

            List<UnitTask> tasks = TaskList.Tasks;
            bool foundTask = false;

            for (int i = tasks.Count - 1; i >= 0 && !foundTask; i--)
            {
                if (tasks[i]._itemToPickUp == other.gameObject)
                {
                    tasks[i].DestroyTask();
                    foundTask = true;
                }
            }
            if (other.tag == "EnergyCrystal")
            {
                WorldController worldController = WorldController.GetWorldController;
                worldController._energyCrystals.Remove(other.gameObject.GetComponent<EnergyCrystal>());
            }

            Destroy(other.gameObject);
        }
    }
}
