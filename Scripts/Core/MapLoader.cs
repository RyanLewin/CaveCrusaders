using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    string LevelName = "";
    void Awake()
    {
        StaticMapInfo.SetLevelLoadData(LevelName, true);
    }
}


