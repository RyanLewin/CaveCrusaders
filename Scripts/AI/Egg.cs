using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Monster
{
    float _spawnTimer;
    float _randomiser;

    [SerializeField] GameObject _cricketPrefab;
    [SerializeField] GameObject _ambushPrefab;
    [SerializeField] GameObject _BigBoyPrefab;
    [SerializeField] GameObject _garyPrefab;


    // Start is called before the first frame update
    void Start()
    {
        Health = 50;
        _spawnTimer = Random.Range(60, 120);
        _randomiser = Random.Range(0, 1000);
        _theWorldControllor = WorldController.GetWorldController;
        _theWorldControllor._monsters.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(Health <= 0)
        {
            _theWorldControllor._monsters.Remove(this);
            Destroy(gameObject);
        }

        if(_spawnTimer <= 0)
        {
            if(_randomiser <= 700)
            {
                //spawn cricket 50% EDIT 70%
                Instantiate(_cricketPrefab, transform.position, _cricketPrefab.transform.rotation);
                _theWorldControllor._monsters.Remove(this);
                Destroy(gameObject);
            }
            else if (_randomiser > 700 && _randomiser <= 999)
            {
                //spawn ambush 30%
                Instantiate(_ambushPrefab, transform.position, _ambushPrefab.transform.rotation);
                _theWorldControllor._monsters.Remove(this);
                Destroy(gameObject);
            }
            //else if (_randomiser > 800 && _randomiser <= 999)
            //{
            //    //spawn big boi 20%
            //    Instantiate(_BigBoyPrefab, transform.position, _BigBoyPrefab.transform.rotation);
            //    _theWorldControllor._monsters.Remove(this);
            //    Destroy(gameObject);
            //}
            else
            {
               //spawn Gary 0.1%
               GameObject gary = Instantiate(_garyPrefab, transform.position, _garyPrefab.transform.rotation);
                gary.GetComponent<Worker>()._name = "Gary";
                gary.GetComponent<Worker>().Health = 200;
                _theWorldControllor._monsters.Remove(this);
                Destroy(gameObject);
            }
        }
        else
        {
            _spawnTimer -= Time.deltaTime;
            transform.localScale = transform.localScale + (new Vector3(0.001f, 0.001f, 0.001f) * Time.deltaTime);
        }
    }
}
