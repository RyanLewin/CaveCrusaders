using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISound : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] List<AudioClip> _attackSound = new List<AudioClip>();
    [SerializeField] List<AudioClip> _randomSound = new List<AudioClip>();
    float _soundWait = 0;
    // Start is called before the first frame update
    void Start()
    {
        _soundWait = Random.Range(10, 30);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_audioSource.isPlaying)
        {
            _soundWait -= Time.deltaTime;

            if (_soundWait <= 0)
            {
                if (_randomSound.Count != 0)
                {
                    _audioSource.clip = _randomSound[Random.Range(0, _randomSound.Count)];
                    _audioSource.Play();
                }
                _soundWait = Random.Range(10, 30);
            }
        }
    }

    public void Attack()
    {
        if (!_audioSource.isPlaying)
        {
            if (_attackSound.Count != 0)
            {
                _audioSource.clip = _attackSound[Random.Range(0, _attackSound.Count)];
                _audioSource.Play();
            }
            _soundWait = Random.Range(10, 30);
        }
    }
    public void PlaySound(AudioClip clip)
    {
        if (!_audioSource.isPlaying)
        {
            if (clip != null)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }
            _soundWait = Random.Range(10, 30);
        }
    }
}
