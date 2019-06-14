using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerVoices : MonoBehaviour
{
    public static WorkerVoices _workerVoices;
      
    //DaveEtte
    public List<AudioClip> _daveEtteSelect;
    public List<AudioClip> _daveEtteOrder;

    //The other DaveEtte
    public List<AudioClip> _theOtherdaveEtteSelect;
    public List<AudioClip> _theOtherdaveEtteOrder;

    //Scottish Daveette
    public List<AudioClip> _scottDaveetteSelect;
    public List<AudioClip> _scottDaveetteOrder;

    //Southern Daveette
    public List<AudioClip> _southernDaveetteSelect;
    public List<AudioClip> _southernDaveetteOrder;

    //Irish Daveette
    public List<AudioClip> _irishDaveetteSelect;
    public List<AudioClip> _irishDaveetteOrder;

    //Dave From Bristol
    public List<AudioClip> _bristolDaveSelect;
    public List<AudioClip> _bristolDaveeOrder;

    //Dave From the south
    public List<AudioClip> _southernDaveSelect;
    public List<AudioClip> _southernDaveeOrder;

    //grumpy dave
    public List<AudioClip> _grumpyDaveSelect;
    public List<AudioClip> _grumpyDaveeOrder;

    //irish dave
    public List<AudioClip> _irishDaveSelect;
    public List<AudioClip> _irishDaveeOrder;

    //the other irish dave
    public List<AudioClip> _theOtherIrishDaveSelect;
    public List<AudioClip> _theOtherIrishDaveeOrder;
    private void Start()
    {
        _workerVoices = this;
    }

    public static WorkerVoices GetWorkerVoices { get { return _workerVoices; } }
}
