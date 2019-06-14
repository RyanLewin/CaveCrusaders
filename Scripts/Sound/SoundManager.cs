using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] List<AudioClip> _backgroundMusic = new List<AudioClip>();
    [SerializeField] List<AudioClip> _battleMusic = new List<AudioClip>();
    [SerializeField] List<AudioClip> _ambientSound = new List<AudioClip>();
    [SerializeField] AudioClip _winMusic;
    [SerializeField] AudioClip _lossMusic;
    [SerializeField] AudioClip _weldone;
    [SerializeField] AudioClip _buttonClick;
    [SerializeField] AudioClip _buttonWhosh;
    [SerializeField] AudioClip _alarm;
    [SerializeField] AudioClip _pixaxe;
    [SerializeField] AudioSource _speechSource;
    [SerializeField] AudioSource _announcementSource;
    [SerializeField] AudioSource _SFXSource;
    [SerializeField] AudioSource _ambientSource;
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _winLossMusic;
    [SerializeField] AudioSource _battleMusicSource;
    [SerializeField] AudioSource _UISource;

    List<GameObject> _lavaSource = new List<GameObject>();
    float _ambentWait = 0;
    float _ambentPlayTime = 0;
    float _musicTime = 0;
    int _lastBackgroundTrack = 0;
    int _lastBattleMusicTrack = 0;
    int _lastAmbientTrack = 0;
    bool _battleMode = false;
    float _musicVolume;
    float _battleMusicTime = 0;
    bool _onWinLoss = false;

    public List<GameObject> LavaSource { get => _lavaSource; }

    public bool BattleMode
    {
        set
        {
            if (!_battleMode && value && _battleMusicSource.volume <= 0)
            {
                if (_battleMusic.Count != 0)
                {
                    _lastBattleMusicTrack = Random.Range(0, _battleMusic.Count);
                    _battleMusicSource.clip = _battleMusic[_lastBattleMusicTrack];
                    _battleMusicTime = _battleMusicSource.clip.length;
                    _battleMusicSource.Play();
                }
            }
            if (!_battleMode)
            {
                _battleMode = value;
            }

        }
    }

    void Start()
    {

        _musicVolume = _musicSource.volume;
        _ambentWait = Random.Range(10, 30);
        if (WorldController.GetWorldController != null)
        {
            WorldController.GetWorldController._soundManager = this;
        }
        if (_backgroundMusic.Count != 0)
        {
            _lastBackgroundTrack = Random.Range(0, _backgroundMusic.Count);
            _musicTime = _backgroundMusic[_lastBackgroundTrack].length;
            _musicSource.clip = _backgroundMusic[_lastBackgroundTrack];
            _musicSource.Play();
        }

    }

    void Update()
    {
        BackGroundMusic();
        if (!_onWinLoss)
        {
            Ambient();
        }
        BattleMusic();
    }

    void BackGroundMusic()
    {
        if (_backgroundMusic.Count > 0)
        {
            _musicTime -= Time.deltaTime;
            if (_musicTime <= 0)
            {
                if (_backgroundMusic.Count == 1)
                {
                    _musicSource.clip = _backgroundMusic[_lastBackgroundTrack];
                    _musicTime = _backgroundMusic[_lastBackgroundTrack].length;
                    _musicSource.Play();
                }
                else
                {
                    int nextTrack = 0;
                    do
                    {
                        nextTrack = Random.Range(0, _backgroundMusic.Count);
                    }
                    while (nextTrack == _lastBackgroundTrack);

                    _lastBackgroundTrack = nextTrack;
                    _musicTime = _backgroundMusic[_lastBackgroundTrack].length;
                    _musicSource.clip = _backgroundMusic[_lastBackgroundTrack];
                    _musicSource.Play();
                }
            }
        }
    }
    void Ambient()
    {
        if (_ambientSound.Count != 0)
        {
            _ambentPlayTime -= Time.deltaTime;
            if (_ambentPlayTime <= 0)
            {
                _ambentWait -= Time.deltaTime;
                if (_ambentWait <= 0)
                {
                    if (_ambientSound.Count == 1)
                    {
                        _ambientSource.clip = _ambientSound[_lastAmbientTrack];
                        _ambentPlayTime = _ambientSound[_lastAmbientTrack].length;
                        _ambientSource.Play();
                    }
                    else
                    {
                        int nextTrack = 0;
                        do
                        {
                            nextTrack = Random.Range(0, _ambientSound.Count);
                        }
                        while (nextTrack == _lastAmbientTrack);

                        _lastAmbientTrack = nextTrack;
                        _ambentPlayTime = _ambientSound[_lastAmbientTrack].length;
                        _ambientSource.clip = _ambientSound[_lastAmbientTrack];
                        _ambientSource.Play();
                    }
                    _ambentWait = Random.Range(10, 30);
                }
            }
        }
    }

    void BattleMusic()
    {
        if (!_onWinLoss)
        {
            if (_battleMode)
            {
                float fade = Time.deltaTime / 100;
                if (_battleMusicSource.volume < _musicVolume)
                {
                    _battleMusicSource.volume += fade;
                    _musicSource.volume -= fade;
                }
                else
                {
                    _musicSource.volume = 0;
                }

            }
            else
            {
                float fade = Time.deltaTime / 100;
                if (_musicSource.volume < _musicVolume)
                {
                    _battleMusicSource.volume -= fade;
                    _musicSource.volume += fade;
                }
                else
                {
                    _battleMusicSource.volume = 0;
                }
            }
        }

        if (_backgroundMusic.Count > 0 && _battleMusicSource.volume != 0)
        {
            _battleMusicTime -= Time.deltaTime;
            if (_battleMusicTime <= 0)
            {
                if (_battleMusic.Count == 1)
                {
                    _battleMusicSource.clip = _battleMusic[_lastBattleMusicTrack];
                    _battleMusicTime = _battleMusic[_lastBattleMusicTrack].length;
                    _battleMusicSource.Play();
                }
                else
                {
                    int nextTrack = 0;
                    do
                    {
                        nextTrack = Random.Range(0, _battleMusic.Count);
                    }
                    while (nextTrack == _lastBattleMusicTrack);

                    _lastBattleMusicTrack = nextTrack;
                    _battleMusicTime = _battleMusic[_lastBattleMusicTrack].length;
                    _battleMusicSource.clip = _battleMusic[_lastBattleMusicTrack];
                    _battleMusicSource.Play();
                }
            }
        }




        _battleMode = false;
    }

    public void WinLossMusic(bool win)
    {
        _onWinLoss = true;
        _musicSource.volume = 0;
        _battleMusicSource.volume = 0;
        if (win)
        {
            _winLossMusic.clip = _winMusic;
            PlayAnnouncement(_weldone);
        }
        else
        {
            _winLossMusic.clip = _lossMusic;
        }
        _winLossMusic.loop = true;
        _winLossMusic.Play();
        
    }

   public void WinLossReturnToGame()
    {
        _onWinLoss = false;
        _winLossMusic.loop = false;
        if (_winLossMusic != null)
        {
            _winLossMusic.Stop();
        }
        if (_battleMode)
        {
            _battleMusicSource.volume = _musicVolume;
        }
        else
        {
            _musicSource.volume = _musicVolume;
        }
    }


    public void ButtonClick()
    {
        PlayUI(_buttonClick);
    }
    public void MenuWhosh()
    {
        PlayUI(_buttonWhosh);
    }

    public void SoundAlarm()
    {
        if (!_SFXSource.isPlaying)
        {
            _SFXSource.PlayOneShot(_alarm, 0.05f);
        }

    }

    public void PlaySFX(AudioClip clip)
    {
        _SFXSource.PlayOneShot(clip,1);
    }

    public void PlayAnnouncement(AudioClip clip)
    {
        _announcementSource.PlayOneShot(clip);
    }

    public void PlaySpeech(AudioClip clip)
    {
        _speechSource.clip = clip;
        _speechSource.Play();
    }
    
    public void PlayUI(AudioClip clip)
    {
        _UISource.PlayOneShot(clip);
    }

    public void SFXTest()
    {
        if (Input.GetMouseButton(0)&& !_SFXSource.isPlaying)
        { 
            _SFXSource.clip = _pixaxe;
        _SFXSource.Play();
    }
    }
    public void SpeechTest()
    {
        if (Input.GetMouseButton(0) && !_announcementSource.isPlaying)
        {
            _announcementSource.clip = _weldone;
            _announcementSource.Play();
        }
    }
    public void UITest()
    {
        if (Input.GetMouseButton(0) && !_UISource.isPlaying)
        {
            _UISource.clip = _buttonClick;
            _UISource.Play();
        }
    }

    public bool SpawnLavaSource(Vector3 newLocation)
    {
        foreach (GameObject lavaSource in _lavaSource)
        {
            if(Vector3.Distance(lavaSource.transform.position, newLocation) < 20)
            {
                return false;
            }
        }
        return true;
    }

}
