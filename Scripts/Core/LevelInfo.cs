using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfo : MonoBehaviour
{
    public static LevelInfo _levelInfo;
    [SerializeField] public Transform _tutorialUI;
    [SerializeField] public ObjectiveMenuScript _objectiveMenu;
    [SerializeField] public GameObject _victoryPanal;
    [SerializeField] public GameObject _defeatPanal, _statsDisplay;
    [SerializeField] public Button _oxengenGenButton, _changeToolButton, _lavaBlockadeButton, _garageButton,_outpostButton,_H2OConverterButton, _turretButton, _powerGeneratorButton;
    [SerializeField] public Text _defeatText;

    // Start is called before the first frame update
    void Awake()
    {
        _levelInfo = this;
    }

  

}
