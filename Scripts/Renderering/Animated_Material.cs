using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Substance.Game;
public class Animated_Material : MonoBehaviour
{

    public SubstanceGraph substanceasset;
    public float flow = 1;
    public float time = 0;
    public int res = 512;
    public bool lava = false;
    SubstanceGraph substance;
    [SerializeField]
    bool Submit = false;
    void Start()
    {
        // GetMat();
        substanceasset.Duplicate();
        //GetComponent<Renderer>().sharedMaterial = substance.material;
        GetMat();
    }
    void GetMat()
    {
        Material mat = this.GetComponent<Renderer>().sharedMaterial;
        substance = SubstanceGraph.Find(mat);
    }

    void Update()
    {
        if (substance == null)
        {
            GetMat();
            Debug.LogWarning("No Graph");
            return;
        }
        time += Time.smoothDeltaTime;
        if (lava)
        {
            substance.SetInputFloat("flow_speed", flow);
        }
        substance.SetInputFloat("$time", time);
        substance.QueueForRender();
    }

    private void LateUpdate()
    {
        if (substance == null)
        {
            return;
        }
        if (Submit)
        {
            substance.RenderAsync();
            Substance.Game.Substance.RenderSubstancesAsync();
        }
    }

}
