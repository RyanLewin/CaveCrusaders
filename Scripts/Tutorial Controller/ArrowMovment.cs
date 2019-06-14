using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMovment : MonoBehaviour
{
    [SerializeField] float _heightChange = 4;
    float _maxHight;
    float _minHight;
    bool _up = true;
    // Start is called before the first frame update
    void Start()
    {
        _maxHight = transform.position.y + (_heightChange / 2);
        _minHight = transform.position.y - (_heightChange / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y > _maxHight || transform.position.y < _minHight)
        {
            _up = !_up;
        }


        float yChange = 2f;
        if(!_up)
        {
            yChange *= -1;
        }
        transform.Translate(new Vector3(0, yChange, 0) * Time.deltaTime, Space.World);
        transform.Rotate(0.25f,0,0);
    }
}
