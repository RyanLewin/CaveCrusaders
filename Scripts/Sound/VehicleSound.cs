using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSound : MonoBehaviour
{
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _mining;
    [SerializeField] AudioClip _laserMining;
    [SerializeField] List<AudioClip> _selectionSounds = new List<AudioClip>();
    [SerializeField] List<AudioClip> _orderSounds = new List<AudioClip>();
    bool isMining = false;
    bool stopedMining = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isMining && !stopedMining)
        {
            stopedMining = true;
            _audioSource.loop = false;
            _audioSource.Stop();
        }
        isMining = false;
    }
    public void Mining()
    {

        if (!_audioSource.isPlaying)
        {
            _audioSource.loop = true;
           
            if (WorldController.GetWorldController._miningLevel != WorldController.MiningLevel.three)
            {
                _audioSource.clip = _mining;
            }
            else
            {
                _audioSource.clip = _laserMining;
            }
            _audioSource.Play();
        }
        isMining = true;
        stopedMining = false;
    }
    public void orderSounds()
    {
        if(_orderSounds.Count != 0 && !_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(_orderSounds[Random.Range(0, _selectionSounds.Count)]);
        }
    }
    public void selectionSound()
    {
        if (_selectionSounds.Count != 0 && !_audioSource.isPlaying)
        {
            
            _audioSource.PlayOneShot(_selectionSounds[Random.Range(0, _selectionSounds.Count)]);
        }
    }
}
