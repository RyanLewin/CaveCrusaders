using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCaveBrute : Monster
{
    RockScript _targetRock;

    // Start is called before the first frame update
    void Start()
    {
        _type = MonsterType.Brute;
        _attackTimer = 1.8f;
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _currentMonsterState = MonsterState.wondering;
        _theWorldControllor = WorldController.GetWorldController;
        _navAgent.speed = _speed;
        _theWorldControllor._monsters.Add(gameObject.GetComponent<MonsterCaveBrute>());
        _hiding = false;

        //health Bar
        _healthBarUpdater = GetComponentInChildren<HealthBarUpdaterScript>();
        _healthBarUpdater._maxHealth = Health;
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            if (!dead)
            {
                WorldController.GetWorldController._levelStatsController.MosterKilled();
                _aiSound.enabled = false;
            }
            dead = true;
        }
        if (!dead)
        {
            _crystalList = _theWorldControllor._energyCrystals;
            _buildingList = _theWorldControllor._buildings;
            _UnitList = _theWorldControllor._workers;

            //main switch
            switch (_currentMonsterState)
            {
                case MonsterState.wondering:
                    if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer <= 0)
                    {
                        _navAgent.destination = new Vector3(Random.Range(transform.position.x - 20, transform.position.x + 20), Random.Range(transform.position.y - 20, transform.position.y + 20), Random.Range(transform.position.z - 20, transform.position.z + 20));
                        _animator.SetBool("Walking", true);
                        _animator.SetBool("Attacking", false);
                        _wonderTimer = 5;
                    }
                    else if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer > 0)
                    {
                        _wonderTimer -= Time.deltaTime;
                        _animator.SetBool("Walking", false);
                    }

                    // CheckWhatsClosest(true,false,30);
                    CheckWhatsClosestMk2(true,false,false,false,30,true);

                    break;
                
                case MonsterState.attackingBuilding:
                    _navAgent.SetDestination(_closestBuilding.transform.position);
                    _animator.SetBool("Walking", true);

                    if (Vector3.Distance(_closestBuilding.transform.position, transform.position) < 5f && !_navAgent.pathPending)
                    {
                        _animator.SetBool("Walking", false);
                        _animator.SetBool("Attacking", true);

                        if (_attackTimer <= 0)
                        {
                           // Debug.Log("Nom!");
                            _closestBuilding.GetComponent<Building>().Health -= _attackDamage;
                            _aiSound.Attack();
                            _attackTimer = 1.8f;
                        }
                        else
                        {
                            _attackTimer -= Time.deltaTime;
                        }
                    }
                    //CheckWhatsClosest(true,false,30);
                    CheckWhatsClosestMk2(true, false, false, false, 30, true);
                    break;
                case MonsterState.attackingUnit:                   

                    if (Vector3.Distance(_closestUnit.transform.position, transform.position) < 3)
                    {
                        _navAgent.speed = _speed;
                        _animator.SetBool("Walking", false);
                        _animator.SetBool("Attacking", true);
                        if (_attackTimer <= 0f)
                        {
                          //  Debug.Log("Nom!");

                            _closestUnit.GetComponent<Unit>().TakeDamage(_attackDamage);
                            _aiSound.Attack();
                            _attackTimer = 1.8f;
                        }
                        else
                        {
                            _attackTimer -= Time.deltaTime;
                        }
                    }
                    else
                    {
                        _navAgent.SetDestination(_closestUnit.transform.position);
                        _animator.SetBool("Walking", true);
                    }
                   // Debug.Log("Remaining distance: " + _navAgent.remainingDistance + " Distance too unit: " + Vector3.Distance(_closestUnit.transform.position, transform.position) + ", is path pending: " + _navAgent.pathPending);
                    //Debug.Log((_navAgent.remainingDistance <= 3) + " " + (Vector3.Distance(_closestUnit.transform.position, transform.position) > 5) + " " + (!_navAgent.pathPending));
                    if (_navAgent.remainingDistance <= 3 && Vector3.Distance(_closestUnit.transform.position,transform.position) > 5 && Vector3.Distance(_closestUnit.transform.position, transform.position) < 28)
                    {
                        _currentMonsterState = MonsterState.BreakWall;
                    //    Debug.Log("Change to break wall");
                        break;
                    }

                    if (_closestUnit == null)
                    {
                        _currentMonsterState = MonsterState.wondering;
                    }
                    if (Vector3.Distance(_closestUnit.transform.position, transform.position) > 20)
                    {
                        _currentMonsterState = MonsterState.wondering;
                    }
                   // CheckWhatsClosest(true,false,30);
                    CheckWhatsClosestMk2(true, false, false, false, 30, true);
                    break;
                case MonsterState.eating:
                    _navAgent.SetDestination(_closestEnergyCrystal.transform.position);
                    _animator.SetBool("Walking", true);
                    if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending)
                    {
                        _animator.SetBool("Walking", false);
                        _theWorldControllor._energyCrystals.Remove(_closestEnergyCrystal);
                        Destroy(_closestEnergyCrystal.transform.gameObject);
                        _eatenCrystals++;
                        _currentMonsterState = MonsterState.wondering;
                    }
                    // CheckWhatsClosest(true,false,30);
                    CheckWhatsClosestMk2(true, false, false, false, 30, true);
                    break;
                case MonsterState.BreakWall:

                    //ray cast to closest worker
                    _ray = new Ray(transform.position, _closestUnit.transform.position - transform.position);
                 //  Debug.DrawRay(transform.position, _closestUnit.transform.position - transform.position);

                    if (Physics.Raycast(_ray, out _hit))
                    {
                        if (_hit.transform.tag == "RockTile")
                        {
                           if(_hit.transform.GetComponentInParent<RockScript>().RockType != RockScript.Type.SolidRock)
                            {
                                _targetRock = _hit.transform.GetComponentInParent<RockScript>();
                                _navAgent.SetDestination(_targetRock.transform.position);
                            }
                        }
                    }

                    if(_targetRock == null)
                    {
                        _currentMonsterState = MonsterState.wondering;
                    }
                    else
                    {
                        if(Vector3.Distance(transform.position,_targetRock.transform.position) < 6)
                        {
                            _targetRock.DistroyRock();
                            _targetRock = null;
                            _currentMonsterState = MonsterState.wondering;
                        }
                    }

                    break;
            }
        }
        else
        {
            WorldController.GetWorldController._monsters.Remove(this);

            _navAgent.ResetPath();
            _navAgent.enabled = false;
            _animator.SetBool("Dead", true);
            _animator.SetBool("Walking", false);
            _animator.SetBool("Attacking", false);
            _animator.SetBool("False", false);
            Destroy(GetComponent<BoxCollider>());
            Destroy(GetComponent<SelectableObject>());
        }
    }
}
