using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaSound : MonoBehaviour
{
    private void Start()
    {
        WorldController worldController = WorldController.GetWorldController;
        if (worldController != null)
        {
            SoundManager soundManager = worldController._soundManager;
            if (soundManager != null)
            {
                soundManager.LavaSource.Add(gameObject);
            }
        }
    }
    void OnDestroy()
    {
        WorldController worldController = WorldController.GetWorldController;
        if(worldController != null)
        {
            SoundManager soundManager = worldController._soundManager;
            if (soundManager != null)
            {
                soundManager.LavaSource.Remove(gameObject);
            }

        }

    }
}
