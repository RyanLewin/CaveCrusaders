using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitToolInfoScript : MonoBehaviour
{
    public static UnitToolInfoScript instance;

    [SerializeField]
    Text _minersText, _buildersText, _diggersText, _shootersText, _flamersText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        int minerCount = 0, builderCount = 0, diggerCount = 0, shooterCount = 0, flamerCount = 0;

        foreach (Unit unit in WorldController.GetWorldController._workers)
        {
            Worker worker = unit.GetComponent<Worker>();
            if (worker != null && !worker.Dead && !worker._inVehicle)
            {
                switch (worker._currentTool)
                {
                    case Unit.UnitTool.MiningTool:
                        minerCount++;
                        break;
                    case Unit.UnitTool.Hammer:
                        builderCount++;
                        break;
                    case Unit.UnitTool.Shovel:
                        diggerCount++;
                        break;
                    case Unit.UnitTool.Weapon:
                        shooterCount++;
                        break;
                    case Unit.UnitTool.FlameThrower:
                        flamerCount++;
                        break;
                }
            }
        }

        SetText(_minersText, minerCount);
        SetText(_buildersText, builderCount);
        SetText(_diggersText, diggerCount);
        SetText(_shootersText, shooterCount);
        SetText(_flamersText, flamerCount);
    }

    void SetText(Text textGO, int textCount)
    {
        textGO.text = textCount.ToString();
        textGO.color = (textCount == 0)? Color.red : Color.white;
    }
}
