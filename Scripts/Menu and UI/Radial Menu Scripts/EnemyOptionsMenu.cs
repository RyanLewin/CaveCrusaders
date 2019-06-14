using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyOptionsMenu : MonoBehaviour
{
    Monster _monster;

    [SerializeField]
    Button _attackButton;
    [SerializeField]
    Text _enemyText;

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            DoUpdateChecks();
        }
    }

    /// <summary>
    /// Open enemy radial dial, store monster
    /// </summary>
    /// <param name="monster">Enemy selected</param>
    public void Open(Monster monster)
    {
        _monster = monster;
        _enemyText.text = monster._type.ToString();
        DoUpdateChecks();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Check for button interactablity or null object and update position
    /// </summary>
    public void DoUpdateChecks()
    {
        if (_monster == null)
        {
            RadialMenuScript.instance.CloseMenu();
            return;
        }

        transform.position = Camera.main.WorldToScreenPoint(_monster.transform.position);

        RadialMenuScript.instance.SetButtonInteractable(_attackButton, ((int)WorldController.GetWorldController._miningLevel >= 1));
        //_attackButton.interactable = ((int)WorldController.GetWorldController._miningLevel >= 1);
    }

    /// <summary>
    /// When selecting to attack, create attack task and add to task list
    /// </summary>
    public void AttackEnemy()
    {
        TaskList.AddTaskToGlobalTaskList(TaskLibrary.Get().CreateTask(UnitTask.TaskType.Attack, _monster.transform.position, _monster.gameObject));

        //    UnitTask tempTask = new UnitTask
        //    {
        //        _location = _monster.transform.position,
        //        _taskType = UnitTask.TaskType.Attack,
        //        _requiredTool = Unit.UnitTool.Weapon,
        //        _taskDescription = "Attacking"
        //    };
        //    AddTaskToGlobalTaskList(tempTask);
    }
}
