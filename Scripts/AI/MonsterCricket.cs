using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterCricket : Monster
{       

    //jump
    float _jumpTimer = 1.8f;
    bool _startJump = false;
    float _jumpSpeed = 3.5f;
    Vector3 _jumpTarget;
    float _jumpDelaytimer = 4;
    float _JumpTrasitionTime = 1.8f;

    // Start is called before the first frame update
    void Start()
    {
        _type = MonsterType.Hopper;
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _currentMonsterState = MonsterState.wondering;
        _theWorldControllor = WorldController.GetWorldController;
        _navAgent.speed = _speed;
        _theWorldControllor._monsters.Add(gameObject.GetComponent<MonsterCricket>());

        //health Bar
        _healthBarUpdater = GetComponentInChildren<HealthBarUpdaterScript>();
        _healthBarUpdater._maxHealth = Health;

        _crystalList = _theWorldControllor._energyCrystals;
        _buildingList = _theWorldControllor._buildings;
        _UnitList = _theWorldControllor._workers;
        _hiding = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_monsterActive)
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
            if (transform.GetChild(0).transform.position.y < 0)
            {
                transform.GetChild(0).transform.Translate(new Vector3(0, 1 * Time.deltaTime, 0));
            }

            if (!dead)
            {

                //main switch
                switch (_currentMonsterState)
                {
                    case MonsterState.wondering:
                        if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer <= 0)
                        {
                            _navAgent.destination = new Vector3(Random.Range(transform.position.x - 20, transform.position.x + 20), Random.Range(transform.position.y - 20, transform.position.y + 20), Random.Range(transform.position.z - 20, transform.position.z + 20));
                            _animator.SetBool("Walking", true);
                            _animator.SetBool("Attacking", false);
                            _animator.SetBool("Jump", false);
                            _wonderTimer = 5;
                        }
                        else if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending && _wonderTimer > 0)
                        {
                            _wonderTimer -= Time.deltaTime;
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Jump", false);
                        }

                        //CheckWhatsClosest(true,true,20);
                        CheckWhatsClosestMk2(false, true, false, true, 20, true);

                        break;

                    case MonsterState.jump:

                        if (!_startJump)
                        {
                            _navAgent.SetDestination(_jumpTarget);
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Jump", true);
                            _startJump = true;
                            _jumpTimer = 2;
                        }

                        if (!_navAgent.pathPending && _JumpTrasitionTime <= 0)
                        {
                            if (CanIGetToLocation(_jumpTarget))
                            {
                                if (transform.GetChild(0).transform.position.y > 0 && _jumpTimer > 0)
                                {
                                    _jumpSpeed -= Time.deltaTime * 4;
                                    _navAgent.speed = _speed * 4;
                                    _jumpTimer -= Time.deltaTime;
                                }
                                else
                                {
                                    _animator.SetBool("Jump", false);
                                    _JumpTrasitionTime = 1.8f;
                                    _currentMonsterState = MonsterState.attackingUnit;
                                    _navAgent.speed = _speed;
                                    _jumpSpeed = 3.5f;
                                    _startJump = false;

                                }

                                if (_navAgent.remainingDistance <= 1 && !_navAgent.pathPending)
                                {
                                    _jumpSpeed -= Time.deltaTime * 10;
                                    _animator.SetBool("Jump", false);
                                }

                                transform.GetChild(0).transform.Translate(new Vector3(0, _jumpSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                _navAgent.ResetPath();
                                _currentMonsterState = MonsterState.attackingUnit;
                                _animator.SetBool("Jump", false);
                            }
                        }
                        else
                        {
                            _JumpTrasitionTime -= Time.deltaTime;
                        }

                        break;
                    case MonsterState.attackingBuilding:

                        if (_closestBuilding != null)
                        {
                            _navAgent.SetDestination(_closestBuilding.transform.position);
                            _animator.SetBool("Walking", true);
                            _animator.SetBool("Jump", false);

                            if (Vector3.Distance(_closestBuilding.transform.position, transform.position) < 5f && !_navAgent.pathPending)
                            {
                                _animator.SetBool("Walking", false);
                                _animator.SetBool("Attacking", true);

                                if (_attackTimer <= 0)
                                {
                                  //  Debug.Log("Nom!");
                                    _aiSound.Attack();
                                    _closestBuilding.GetComponent<Building>().Health -= _attackDamage;
                                    _attackTimer = 2f;
                                }
                                else
                                {
                                    _attackTimer -= Time.deltaTime;
                                }
                            }
                        }
                        //CheckWhatsClosest(true,true,20);
                        CheckWhatsClosestMk2(false, true, false, true, 20, true);
                        break;
                    case MonsterState.attackingUnit:
                        if (_closestUnit != null)
                        {
                            if (_jumpDelaytimer <= 0 && Vector3.Distance(_closestUnit.transform.position, transform.position) <= 14 && Vector3.Distance(_closestUnit.transform.position, transform.position) >= 13)
                            {
                                _currentMonsterState = MonsterState.jump;
                                _jumpTarget = new Vector3(_closestUnit.transform.position.x, _closestUnit.transform.position.y, _closestUnit.transform.position.z);
                                _navAgent.ResetPath();
                                _jumpDelaytimer = 4;
                                break;
                            }
                            else
                            {
                                _jumpDelaytimer -= Time.deltaTime;
                            }
                        }

                        if (Vector3.Distance(_closestUnit.transform.position, transform.position) < 3)
                        {
                            _navAgent.speed = _speed;
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Jump", false);
                            _animator.SetBool("Attacking", true);
                            transform.LookAt(new Vector3(_closestUnit.transform.position.x, transform.position.y, _closestUnit.transform.position.z));
                            if (_attackTimer <= 0)
                            {
                               // Debug.Log("Nom!");
                                _aiSound.Attack();
                                _closestUnit.GetComponent<Unit>().TakeDamage(_attackDamage);
                                _attackTimer = 2f;
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
                            _animator.SetBool("Jump", false);
                            _animator.SetBool("Attacking", false);
                        }


                        if (_closestUnit == null)
                        {
                            _currentMonsterState = MonsterState.wondering;
                        }
                        if (Vector3.Distance(_closestUnit.transform.position, transform.position) > 20)
                        {
                            _currentMonsterState = MonsterState.wondering;
                        }
                        // CheckWhatsClosest(true,false,20);
                        CheckWhatsClosestMk2(false, true, false, true, 20, true);
                        break;
                    case MonsterState.eating:
                        _navAgent.SetDestination(_closestEnergyCrystal.transform.position);
                        _animator.SetBool("Walking", true);
                        _animator.SetBool("Jump", false);
                        if (_navAgent.remainingDistance < 0.1f && !_navAgent.pathPending)
                        {
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Jump", false);
                            _theWorldControllor._energyCrystals.Remove(_closestEnergyCrystal);
                            Destroy(_closestEnergyCrystal.transform.gameObject);
                            _eatenCrystals++;
                            _currentMonsterState = MonsterState.wondering;
                        }
                        //CheckWhatsClosest(true,false,20);
                        CheckWhatsClosestMk2(false, true, false, true, 20, true);
                        break;
                }
            }
            else
            {
                WorldController.GetWorldController._monsters.Remove(this);
                if (_navAgent.isOnNavMesh)
                {
                    _navAgent.ResetPath();
                }
                _navAgent.enabled = false;
                _animator.SetBool("Jump", false);
                _animator.SetBool("Walking", false);
                _animator.SetBool("Attacking", false);
                _animator.SetBool("Dead", true);
                Destroy(GetComponent<BoxCollider>());
                Destroy(GetComponent<SelectableObject>());

                if (_eatenCrystals > 0)
                {
                    for (int i = 0; i < _eatenCrystals; i++)
                    {
                        Instantiate(_crystalPrefab, transform.position, _crystalPrefab.transform.rotation);
                    }
                }
            }
        }


    }
    
}
