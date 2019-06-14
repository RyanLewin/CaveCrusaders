using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CumbleSound : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    float _time;
    // Start is called before the first frame update
    void Start()
    {
        _time = _audioSource.clip.length;
    }

    // Update is called once per frame
    void Update()
    {
        _time -= Time.deltaTime;
        if(_time <= 0)
        {
            Object.Destroy(this);
        }
    }
}
