using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistroyPartical : MonoBehaviour
{
    float _waitTime = 3f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _waitTime -= Time.deltaTime;
        if(_waitTime <= 0)
        {
            Object.Destroy(gameObject);
        }
    }
}
