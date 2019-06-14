using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarsUpdaterScript : MonoBehaviour
{
    [SerializeField]
    Image _healthFillBar, _energyFillBar;
    public float _maxHealth;
    int _maxEnergy = 100;
    Camera cam;
    /// <summary>
    /// Update health display
    /// </summary>
    /// <param name="health"></param>
    public void UpdateHealthFill(float health)
    {
        _healthFillBar.fillAmount = (health / _maxHealth);
    }
    private void Start()
    {
        cam = Camera.main;
    }
    /// <summary>
    /// Update energy display
    /// </summary>
    /// <param name="energy"></param>
    public void UpdateEnergyFill(float energy)
    {
        _energyFillBar.fillAmount = (energy / _maxEnergy);
    }

    void LateUpdate()
    {
        transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
