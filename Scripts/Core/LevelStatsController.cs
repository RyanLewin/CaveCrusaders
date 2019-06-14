using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelStatsController : MonoBehaviour
{
    bool _gamePlaying = true;
    float _gameTime = 0;
    int _monstersKilled = 0;
    int _unitsKilled = 0;
    int _units = 0;
    int _mushroomsBurned = 0;
    int _oreColleded = 0;
    int _oreUsed = 0;
    int _energyCrystalCollected = 0;
    int _energyCrystalUsed = 0;
    [SerializeField] Text _timeText, _monstersKilledText, _unitsKilledText, _unitsSpawnedText, _mushroomsBurnedText, _oreCollectedText, _oreSpentText, _crystalsCollectedText, _crystalsSpentText;
    

    // Start is called before the first frame update
    void Start()
    {
        WorldController.GetWorldController._levelStatsController = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(_gamePlaying)
        {
            _gameTime += Time.deltaTime;
        }
    }
    public void GameEnd()
    {
        _gamePlaying = false;
        string minutes = Mathf.Floor(_gameTime / 60).ToString("00");
        string seconds = (_gameTime % 60).ToString("00");

        _timeText.text = string.Format("{0}:{1}", minutes, seconds);
        _monstersKilledText.text = _monstersKilled.ToString();
        _unitsKilledText.text = _unitsKilled.ToString();
        _unitsSpawnedText.text = _units.ToString();
        _mushroomsBurnedText.text = _mushroomsBurned.ToString();
        _oreCollectedText.text = _oreColleded.ToString();
        _oreSpentText.text = _oreUsed.ToString();
        _crystalsCollectedText.text = _energyCrystalCollected.ToString();
        _crystalsSpentText.text = _energyCrystalUsed.ToString();
    }

    public void MosterKilled()
    {
        _monstersKilled++;
    }
    public void UnitKilled()
    {
        _unitsKilled++;
    }
    public void  UnitSpawned()
    {
        _units++;
    }
    public void MushroomBurned()
    {
        _mushroomsBurned++;
    }
    public void OreColleded()
    {
        _oreColleded++;
    }
    public void OreUsed(int oreUsed)
    {
        _oreUsed += oreUsed;
    }
    public void EnergyCrystalCollected()
    {
        _energyCrystalCollected++;
    }
    public void EnergyCrystalUsed()
    {
        _energyCrystalUsed++;
    }
}
