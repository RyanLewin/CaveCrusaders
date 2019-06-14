using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class OxygenGenerator : Building
{
    List<Worker> _inUseBy = new List<Worker>();
    [SerializeField]
    protected VisualEffect _visualEffect;

    protected override void OnTileStart ()
    {
        base.OnTileStart();
        _visualEffect.Stop();
    }

    public void ChangeInWorkers (bool add, Worker worker)
    {
        if (add)
        {
            if (_inUseBy.Contains(worker))
            {
                return;
            }

            _inUseBy.Add(worker);
            _visualEffect.Play();
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.Play("Spray");
            }
        }
        else
        {
            if (!_inUseBy.Contains(worker))
            {
                return;
            }
            _inUseBy.Remove(worker);
            if (_inUseBy.Count <= 0)
            {
                _visualEffect.Stop();
                foreach (Animator anim in GetComponentsInChildren<Animator>())
                {
                    anim.Play("New State");
                }
            }
        }
    }

    public override int GetID()
    {
        return (int)TileTypeID.OxyGen;
    }
}
