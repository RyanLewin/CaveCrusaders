using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAmbush : Monster
{
    Vector3 _hidingPlace;
    float _tirednessTimer;
    

    // Start is called before the first frame update
    void Start()
    {
        _hiding = true;
        _tirednessTimer = 30;
        _type = MonsterType.Ambush;
        _animator = GetComponentInChildren<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();
        _theWorldControllor = WorldController.GetWorldController;
        _navAgent.speed = _speed;
        _theWorldControllor._monsters.Add(this);

        //health Bar
        if (_healthBarUpdater != null)
        {
            _healthBarUpdater = GetComponentInChildren<HealthBarUpdaterScript>();
            _healthBarUpdater._maxHealth = Health;
        }

        _crystalList = _theWorldControllor._energyCrystals;
        _buildingList = _theWorldControllor._buildings;
        _UnitList = _theWorldControllor._workers;
        _currentMonsterState = MonsterState.Hiding;
        _animator.SetBool("Submurged", true);
        
        

        _hidingPlace = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Health <=0)
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
            //main switch
            switch (_currentMonsterState)
            {
                case MonsterState.Hiding:

                    if (_hiding)
                    {
                        _healthBarUpdater.gameObject.SetActive(false);
                        _navAgent.ResetPath();
                        if (_tirednessTimer >= 30)
                        {
                            // CheckWhatsClosest(true, true, 20);
                            CheckWhatsClosestMk2(false, true, false, true, 20, false);
                        }
                        else
                        {
                            _tirednessTimer += Time.deltaTime;
                            _animator.SetBool("Submurged", true);
                        }
                    }
                    else
                    {
                        _navAgent.SetDestination(_hidingPlace);
                        _animator.SetBool("Walking", true);
                        _animator.SetBool("Attacking", false);
                        if (_navAgent.remainingDistance <= 1 && !_navAgent.pathPending)
                        {                            
                            _hiding = true;
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Submurged", true);
                        }
                    }

                    break;
                case MonsterState.wondering:
                    _hiding = false;
                    _healthBarUpdater.gameObject.SetActive(true);
                    _animator.SetBool("Submurged", false);

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

                    _tirednessTimer -= Time.deltaTime;
                    CheckWhatsClosestMk2(false,true,false,true,20,false);
                    //CheckWhatsClosest(true, true, 20);

                    if(_tirednessTimer <= 0)
                    {
                        _currentMonsterState = MonsterState.Hiding;

                    }

                    break;                

                case MonsterState.attackingBuilding:
                    _hiding = false;
                    _animator.SetBool("Submurged", false);
                    _healthBarUpdater.gameObject.SetActive(true);

                    if (_closestBuilding != null)
                    {
                        _navAgent.SetDestination(_closestBuilding.transform.position);
                        _animator.SetBool("Walking", true);

                        if (Vector3.Distance(_closestBuilding.transform.position, transform.position) < 6f && !_navAgent.pathPending)
                        {
                            _animator.SetBool("Walking", false);
                            _animator.SetBool("Attacking", true);

                            if (_attackTimer <= 0)
                            {
                               // Debug.Log("Nom!");
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
                    _tirednessTimer -= Time.deltaTime;
                    CheckWhatsClosestMk2(false, true, false, true, 20, false);
                    //CheckWhatsClosest(true, true, 20);

                    if (_tirednessTimer <= 0)
                    {
                        _currentMonsterState = MonsterState.Hiding;
                    }
                    break;
                case MonsterState.attackingUnit:
                    _hiding = false;
                    _animator.SetBool("Submurged", false);
                    _healthBarUpdater.gameObject.SetActive(true);

                    if (Vector3.Distance(_closestUnit.transform.position, transform.position) < 4)
                    {
                        _navAgent.speed = _speed;
                        _animator.SetBool("Walking", false);
                        _animator.SetBool("Attacking", true);
                        if (_attackTimer <= 0f)
                        {
                          //  Debug.Log("Nom!");
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
                    CheckWhatsClosestMk2(false, true, false, true, 20, false);
                    //CheckWhatsClosest(true, false, 20);
                    _tirednessTimer -= Time.deltaTime;
                    if (_tirednessTimer <= 0)
                    {
                        _currentMonsterState = MonsterState.Hiding;
                    }

                    break;
                case MonsterState.eating:
                    _hiding = false;
                    _healthBarUpdater.gameObject.SetActive(true);
                    _animator.SetBool("Submurged", false);
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
                    _tirednessTimer -= Time.deltaTime;
                    CheckWhatsClosestMk2(false, true, false, true, 20, false);
                    //CheckWhatsClosest(true, false, 20);

                    if (_tirednessTimer <= 0)
                    {
                        _currentMonsterState = MonsterState.Hiding;
                    }
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
            _animator.SetBool("Walking", false);
            _animator.SetBool("Attacking", false);
            _animator.SetBool("Submurged", false);
            _animator.SetBool("Dead", true);
            Destroy(GetComponent<SelectableObject>());
            Destroy(GetComponent<BoxCollider>());

            if (_eatenCrystals > 0)
            {
                for (int i = 0; i < _eatenCrystals; i++)
                {
                    Instantiate(_crystalPrefab, transform.position, _crystalPrefab.transform.rotation);
                }
            }
        }
    }
    public new void TakeDamage(int ammount)
    {
        if (!_hiding)
        {
            Health -= ammount;
        }
    }
}
