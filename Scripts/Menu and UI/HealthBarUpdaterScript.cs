using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUpdaterScript : MonoBehaviour
{
    [SerializeField]
    Image _fillBar;
    public float _maxHealth;
    CPU_FOW FOW;
    Camera Cam;
    [SerializeField]
    bool NeedsFOWCheck = true;
    private void Start()
    {
        Cam = Camera.main;
        FOW = CPU_FOW.Get();
    }
    /// <summary>
    /// Update health display
    /// </summary>
    /// <param name="health"></param>
    public void UpdateFill(float health)
    {
        _fillBar.fillAmount = (health / _maxHealth);
    }

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(0, Cam.transform.eulerAngles.y, 0);
        if (FOW != null && NeedsFOWCheck)
        {
            SetVisible(FOW.SampleFOW(transform.position));
        }
    }

    void SetVisible(bool state)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(state);
        }
    }
}
