using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var range = GetComponentInParent<Building>().range;
        transform.localScale = new Vector3(range * 2, 32, range * 2);
    }
}
