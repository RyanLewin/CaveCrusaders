using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class FOWReceiver : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    float UpdateRate = 0.1f;
    float CurrentCoolDown = 0.0f;
    [SerializeField]
    bool UseMultiSampleFOW = false;
    [SerializeField]
    float MultiSampleRange = 4.0f;
    [Header("Targets")]
    [SerializeField]
    GameObject TargetGameObject;
    [SerializeField]
    VisualEffect Vfx;
    CPU_FOW FOW;
    bool CurrentFOWState = false;
    bool LastState = false;
    // Start is called before the first frame update
    void Start()
    {
        FOW = CPU_FOW.Get();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentCoolDown -= Time.deltaTime;
        if (CurrentCoolDown > 0.0f)
        {
            return;
        }
        CurrentCoolDown = UpdateRate;
        LastState = CurrentFOWState;
        if (UseMultiSampleFOW)
        {
            CurrentFOWState = FOW.MultiSampleFOW(transform.position, MultiSampleRange);
        }
        else
        {
            CurrentFOWState = FOW.SampleFOW(transform.position);
        }
        SetState(CurrentFOWState);
    }

    void SetState(bool Visible)
    {
        if (TargetGameObject != null)
        {
            TargetGameObject.SetActive(Visible);
        }
        if (Vfx != null)
        {
            if (Visible)
            {
                if (LastState == false)
                {
                    Vfx.Play();
                }
            }
            else
            {
                Vfx.Stop();
            }
        }
    }
}
