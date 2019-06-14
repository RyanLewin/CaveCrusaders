using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    Vector3 _targetPosition;
    Vector3 _vector;
    public int damage = 10;
    

    void Start()
    {
        
    } 
    public void SetTarget(Vector3 newTarget)
    {
        _targetPosition = newTarget;
        transform.LookAt(newTarget);
        //_vector = Vector3.MoveTowards(transform.position, _targetPosition, 1f); 
        _vector =   _targetPosition - transform.position;
    }
    void Update()
    {
        transform.position += _vector.normalized;    
    }
    void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Bullet Collision");
        if(collision.transform.tag == "Monster")
        {
            collision.gameObject.GetComponent<Monster>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if(collision.transform.tag != "Floor" && collision.transform.tag != "Worker" && collision.transform.tag != "BURN" && collision.transform.tag != "Bullet" && collision.transform.tag != "Rubble")
        {
            Destroy(gameObject);
        }

    }
}
