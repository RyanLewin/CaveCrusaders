using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IGridNotify : MonoBehaviour
{
    public virtual void OnGridUpdate(){}
    //if needed
    public virtual void OnGridDestruction() { }
    public virtual void OnGridCreationFinished() { }
}
