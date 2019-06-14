using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    TutorialController _tutorial;
    public int _checkpointNo;

    private void Awake ()
    {
        _tutorial = GetComponentInParent<TutorialController>();
    }

    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Worker" || other.tag == "Ore" || other.tag == "EnergyCrystal" || other.tag == "Rubble")
        {
            Debug.Log("Checkpoint reached");
            if (_tutorial.CheckpointReached(this, other.tag))
            {

            //    GetComponent<Collider>().enabled = false;
                Collider[] childCollider = GetComponentsInChildren<Collider>();
                for (int i = 0; i < childCollider.Length; i++)
                {
                    childCollider[i].enabled = false;
                }
            }
        }
    }
}
