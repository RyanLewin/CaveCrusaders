using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class FluidParticleSystemManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    GameObject SystemPreFab = null;
    [Header("Config")]
    [SerializeField]
    int InitalPoolSize = 10;
    [SerializeField]
    int MaxPoolSize = 100;
    [SerializeField]
    float SystemLength = 2.0f;
    [SerializeField]
    float SystemEmitLength = 0.5f;
    [SerializeField]
    float YHeight = 0.0f;
    List<GameObject> ParticleSystemPool = new List<GameObject>();
    List<int> FreeIndex = new List<int>();
    GameObject Parent = null;
    class FluidParticleSystem
    {
        public Vector3 Pos;
        public float Amount;
        public int index;
        public float TimeLeft;
    }
    List<FluidParticleSystem> CurrentSystems = new List<FluidParticleSystem>();
    private void Start()
    {
        Parent = new GameObject("VFX Parent");
        InitalisePool(InitalPoolSize);
    }
    bool FindAndUpdatePS(FluidParticleSystem f)
    {
        for (int i = 0; i < CurrentSystems.Count; i++)
        {
            if (CurrentSystems[i].Pos == f.Pos)
            {
                CurrentSystems[i].TimeLeft = SystemLength;
                return true;
            }
        }
        return false;
    }
    public void NotifyFluidInteraction(Vector3 pos, float amt)
    {
        FluidParticleSystem FPS = new FluidParticleSystem();
        FPS.Pos = pos;
        FPS.Amount = amt;
        FPS.TimeLeft = SystemLength;
        FPS.index = -1;
        if (FindAndUpdatePS(FPS))
        {
            return;
        }
        CurrentSystems.Add(FPS);
    }

    void InitalisePool(int count)
    {
        for (int i = 0; i < InitalPoolSize; i++)
        {
            AddToPool();
        }
    }

    GameObject AddToPool()
    {
        if (ParticleSystemPool.Count > MaxPoolSize)
        {
            return null;
        }
        GameObject NewPS = Instantiate(SystemPreFab, Parent.transform);
        NewPS.SetActive(false);
        ParticleSystemPool.Add(NewPS);
        FreeIndex.Add(ParticleSystemPool.Count - 1);
        return NewPS;
    }

    GameObject GetFromPool(FluidParticleSystem F)
    {
        if (FreeIndex.Count == 0)
        {
            AddToPool();
            if (FreeIndex.Count == 0)
            {
                Debug.Log("Pool full");
                return null;
            }
        }
        int index = FreeIndex[0];
        F.index = index;
        FreeIndex.RemoveAt(0);
        return ParticleSystemPool[index];
    }

    void PlaceSystem(FluidParticleSystem F)
    {
        GameObject Ps = GetFromPool(F);
        if (Ps == null)
        {
            return;
        }
        Ps.SetActive(true);
        F.Pos.y = YHeight;
        Ps.transform.position = F.Pos;
        VisualEffect Effect = Ps.GetComponent<VisualEffect>();
        if (Effect != null)
        {
            Effect.Stop();
            Effect.Reinit();
            Effect.Play();
        }
        else
        {
            Debug.Log("Prefab Missing VFX");
        }

    }

    void ReleaseBackToPool(FluidParticleSystem F)
    {
        if (F.index == -2)
        {
            Debug.LogError("ReleaseBackToPool called twice");
            return;
        }
        GameObject Ps = ParticleSystemPool[F.index];
        Ps.SetActive(false);
        FreeIndex.Add(F.index);
        F.index = -2;
    }
    void SetEmitState(FluidParticleSystem F, bool state)
    {
        GameObject Ps = ParticleSystemPool[F.index];
        if (Ps == null)
        {
            return;
        }
        VisualEffect Effect = Ps.GetComponent<VisualEffect>();
        if (Effect != null)
        {
            if (state)
            {
                Effect.Play();
            }
            else
            {
                Effect.Stop();
            }
        }
        else
        {
            Debug.Log("Prefab Missing VFX");
        }
    }

    private void Update()
    {
        return;
        for (int i = 0; i < CurrentSystems.Count; i++)
        {
            if (CurrentSystems[i].index == -1)
            {
                PlaceSystem(CurrentSystems[i]);
            }
            else
            {
                CurrentSystems[i].TimeLeft -= Time.deltaTime;
                if (CurrentSystems[i].TimeLeft <= 0.0f)
                {
                    ReleaseBackToPool(CurrentSystems[i]);
                    CurrentSystems.RemoveAt(i);
                }
                else if (CurrentSystems[i].TimeLeft <= SystemLength - SystemEmitLength)
                {
                    SetEmitState(CurrentSystems[i], false);
                }
                else
                {
                    SetEmitState(CurrentSystems[i], true);
                }
            }

        }
    }
}
