using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerSound : MonoBehaviour
{
    [SerializeField] AudioSource _workerSFXSource;
    [SerializeField] Animator _animator;
    [SerializeField] AudioClip _pickHit;
    [SerializeField] AudioClip _drillHit;
    [SerializeField] AudioClip _laserDrill;
    [SerializeField] List<AudioClip> _shovel = new List<AudioClip>();
    [SerializeField] AudioClip _footstep;
    [SerializeField] AudioClip _laserPistol;
    [SerializeField] AudioClip _humming;
    [SerializeField] AudioClip _flamer;
    WorldController _worldController;
    bool _drillEffect = false;
    bool _hummingEffect = false;
    bool _flameEffect = false;

    void Start()
    {
        _worldController = WorldController.GetWorldController;
    }
    void Update()
    {
        CheckAnimation();
    }
    public void PlayPickSound()
    {
        _workerSFXSource.PlayOneShot(_pickHit);
    }

    public void PlayDrillSound()
    {
        if (!_drillEffect)
        {
            _drillEffect = true;
            if (_worldController._miningLevel == WorldController.MiningLevel.three)
            {
                _workerSFXSource.clip = _laserDrill;
            }
            else
            {
                _workerSFXSource.clip = _drillHit;
            }
            _workerSFXSource.loop = true;

            _workerSFXSource.Play();
        }

    }

    public void PlayShovelSound()
    {
        _workerSFXSource.PlayOneShot(_shovel[Random.Range(0, _shovel.Count)]);
    }

    public void CheckAnimation()
    {
        if (_drillEffect && !_animator.GetBool("Mining"))
        {
            _drillEffect = false;
            _workerSFXSource.loop = false;
            _workerSFXSource.Stop();
        }
        if (_hummingEffect && !_animator.GetBool("Carrying"))
        {
            _hummingEffect = false;
            _workerSFXSource.loop = false;
            _workerSFXSource.Stop();
        }
        if (_flameEffect && !_animator.GetBool("Flame"))
        {
            StopFlamethrower();
        }
    }

    public void PlayFootstep()
    {
        _workerSFXSource.PlayOneShot(_footstep);
    }

    public void PlayLaserSound()
    {
        _workerSFXSource.PlayOneShot(_laserPistol);
    }

    public void PlayHumming()
    {
        if (!_hummingEffect)
        {
            _hummingEffect = true;

            _workerSFXSource.clip = _humming;

            _workerSFXSource.loop = true;

            _workerSFXSource.Play();
        }
    }
    public void Flamethrower()
    {
        if (!_flameEffect)
        {
            _flameEffect = true;
            _workerSFXSource.clip = _flamer;

            _workerSFXSource.loop = true;

            _workerSFXSource.Play();
        }
    }
    public void StopFlamethrower()
    {
        _flameEffect = false;
        _workerSFXSource.loop = false;
        _workerSFXSource.Stop();
    }
}
