using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    float _daSpinnerSpeed;
    bool _daSpinnerSwitch;

    // Start is called before the first frame update
    void Start()
    {
        _daSpinnerSwitch = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(_daSpinnerSwitch)
        {
            _daSpinnerSpeed -= Time.deltaTime*2;

            if(_daSpinnerSpeed < -45)
            {
                _daSpinnerSwitch = false;
            }
        }
        else
        {
            _daSpinnerSpeed += Time.deltaTime*2;

            if (_daSpinnerSpeed >= 1)
            {
                _daSpinnerSwitch = true;
            }
        }

        transform.Rotate(new Vector3(_daSpinnerSpeed,0,0));
    }
}
