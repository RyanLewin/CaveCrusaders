using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.VFX;

public class MonsterSlug : Monster
{
    VisualEffect smokeVFX;
    bool _evolve;
    float _eggspawnTimer = 2;
    bool _eggspawned = false;

    // Start is called before the first frame update
    void Start()
    {
        smokeVFX = GetComponent<VisualEffect>();
        smokeVFX.Stop();
        _evolve = false;

        _type = MonsterType.Slug;
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _currentMonsterState = MonsterState.wondering;
        _theWorldControllor = WorldController.GetWorldController;
        _navAgent.speed = _speed;
        _theWorldControllor._monsters.Add(gameObject.GetComponent<MonsterSlug>());

        //health Bar
        _healthBarUpdater = GetComponentInChildren<HealthBarUpdaterScript>();
        _healthBarUpdater._maxHealth = Health;

        _buildingList = _theWorldControllor._buildings;
        _wonderTimer = 0;
        _UnitList = _theWorldControllor._workers;
        _evolutionTimer = Random.Range(100, 140);
        _hiding = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            if (!_evolve)
            {
                _evolutionTimer -= Time.deltaTime;

                if (_evolutionTimer <= 0)
                {
                    _evolve = true;
                }

                switch (_currentMonsterState)
                {
                    case MonsterState.wondering:
                        if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer <= 0)
                        {
                            _navAgent.destination = new Vector3(Random.Range(transform.position.x - 20, transform.position.x + 20), Random.Range(transform.position.y - 20, transform.position.y + 20), Random.Range(transform.position.z - 20, transform.position.z + 20));
                            _animator.SetBool("Walking", true);
                            _wonderTimer = Random.Range(3, 7);
                        }
                        else if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer > 0)
                        {
                            _wonderTimer -= Time.deltaTime;
                            _animator.SetBool("Walking", false);
                        }

                        FindClosestUnit();
                        if (Vector3.Distance(_closestUnit.transform.position, transform.position) < 10)
                        {
                            //ray cast to closest Unit
                            _ray = new Ray(transform.position, _closestUnit.transform.position - transform.position);
                            Debug.DrawRay(transform.position, _closestUnit.transform.position - transform.position);

                            if (Physics.Raycast(_ray, out _hit))
                            {
                                if (_hit.transform.tag == "Worker" || _hit.transform.tag == "Unit" || _hit.transform.tag == "Vehicle")
                                {
                                    _currentMonsterState = MonsterState.Flee;
                                }
                            }
                        }

                        break;
                    case MonsterState.Flee:
                        if (Vector3.Distance(_closestUnit.transform.position, transform.position) < 10)
                        {
                            _navAgent.SetDestination(transform.position - (_closestUnit.transform.position - transform.position));
                            _animator.SetBool("Walking", true);
                        }
                        else
                        {
                            _currentMonsterState = MonsterState.wondering;
                        }
                        break;
                }
                if (Health <= 0)
                {
                    _animator.SetBool("Walking", false);
                    _animator.SetBool("Dead", true);
                    Destroy(GetComponent<SelectableObject>());

                    if (!dead)
                    {
                        WorldController.GetWorldController._levelStatsController.MosterKilled();
                        _aiSound.enabled = false;
                    }
                    WorldController.GetWorldController._monsters.Remove(GetComponent<MonsterSlug>());
        
                    _navAgent.ResetPath();
                    dead = true;
                }
            }
            else
            {
                Evolve();
            }
        }
        
    }
    void FindClosestUnit()
    {
        if (_UnitList.Count > 0)
        {
            _closestUnit = _UnitList[0];
            foreach (Unit scaryThing in _UnitList)
            {
                if (Vector3.Distance(scaryThing.transform.position, transform.position) < Vector3.Distance(transform.position, _closestUnit.transform.position))
                {
                    _closestUnit = scaryThing;
                }
            }
        }        
    }
    void Evolve()
    {
        //spawn particle effect
        smokeVFX.Play();
        _navAgent.ResetPath();

        if (_eggspawnTimer <= 0)
        {
            //spawn egg
            if (!_eggspawned)
            {
                Instantiate(_eggPrefab, transform.position, _eggPrefab.transform.rotation);
                GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                _eggspawned = true;
            }
            //delete self
            if (_eggspawnTimer <= -2)
            {
                WorldController.GetWorldController._monsters.Remove(GetComponent<Monster>());
                Destroy(gameObject);
            }
            else
            {
                _eggspawnTimer -= Time.deltaTime;
            }

            if(_eggspawnTimer <= -1)
            {
                smokeVFX.Stop();
            }
        }
        else
        {
            _eggspawnTimer -= Time.deltaTime;
        }
    }
}
