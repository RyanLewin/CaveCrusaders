using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Building
{
    float _shotTimer = 0;
    [SerializeField] GameObject _bulletPrefab;
    Monster _closestMonster;
    [SerializeField] Transform _firePoint;
    [SerializeField] Transform _rotatable;
    [SerializeField] Transform _rotatePoint;
    [SerializeField] AudioSource _shootAuidoSouce;
    [SerializeField] AudioClip _shoot;
    float _distance;
    
    int _damage = 25;

    private void Awake ()
    {
        //_rotatable.parent = _rotatePoint;
    }

    protected override void TileUpdate ()
    {
        base.TileUpdate();
        if (_Energy > 0 && _Built)
        {
            if (_worldController._monsters.Count <= 0)
                return;
            if (FindClosestMonster() != null)
            {
                if (_distance < range)
                {
                    _rotatePoint.LookAt(_closestMonster.transform.position);
                    _rotatable.rotation = Quaternion.RotateTowards(_rotatable.rotation, _rotatePoint.rotation, 5f);
                    if (Quaternion.Angle(_rotatePoint.rotation, _rotatable.rotation) < .5f)
                    {
                        Fire();
                    }
                }
            }
        }
    }

    void Fire ()
    {
        if (_shotTimer <= 0)
        {
            _shootAuidoSouce.PlayOneShot(_shoot);

            GameObject temp = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
            temp.GetComponent<LaserBullet>().SetTarget(_closestMonster.transform.position);
            temp.GetComponent<LaserBullet>().damage = _damage;
            ReduceEnergy(5);

            if (_Level == 0)
            {
                _shotTimer = 3f;
            }
            else if (_Level == 1)
            {
                _shotTimer = 1.5f;
            }
            else if (_Level == 2)
            {
                _shotTimer = .5f;
            }
        }
        else
        {
            _shotTimer -= Time.deltaTime;
        }
    }

    Monster FindClosestMonster ()
    {
        if (_worldController._monsters.Count > 0)
        {
            _closestMonster = _worldController._monsters[0];
            _distance = Vector3.Distance(_closestMonster.transform.position, transform.position);
            foreach (Monster theMonster in _worldController._monsters)
            {
                float newDistance = Vector3.Distance(theMonster.transform.position, transform.position);
                if (newDistance < _distance)
                {
                    _closestMonster = theMonster;
                    _distance = newDistance;
                }
            }
            return _closestMonster;
        }
        else
        {
            _closestMonster = null;
            return null;
        }
    }

    public override int GetID ()
    {
        return (int)TileTypeID.Turret;
    }
}
