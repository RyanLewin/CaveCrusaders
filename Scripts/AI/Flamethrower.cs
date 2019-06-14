using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class Flamethrower : MonoBehaviour
{
    [SerializeField]
    VisualEffect flame;

    private void Awake ()
    {
        GetComponent<BoxCollider>().enabled = false;
        flame.Stop();
    }

    public void TurnOnOff (bool on)
    {
        if (on)
        {
            GetComponent<BoxCollider>().enabled = true;
            flame.Play();
        }
        else
        {
            GetComponent<BoxCollider>().enabled = false;
            flame.Stop();
        }
    }
}
