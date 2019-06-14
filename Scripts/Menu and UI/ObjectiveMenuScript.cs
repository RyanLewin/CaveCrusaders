using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveMenuScript : MonoBehaviour
{
    public static ObjectiveMenuScript instance;

    public List<Objective> ObjectivesList { get; set; }

    [SerializeField]
    GameObject _objectivesContentView, _objectiveContentItemPrefab;
    [SerializeField]
    Text _objectivesButtonText;

    bool _doOnce;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Initialise();
    }

    /// <summary>
    /// Initialise objective list
    /// </summary>
    public void Initialise()
    {
        /*TODO do this in tutorial controller*/
        ObjectivesList = new List<Objective>
        {
            //new Objective("Collect 10 energy crystals", true, 10),
            //new Objective("Upgrade HQ to level 2", false, 0),
        };

        UpdateObjectiveList();
    }

    /// <summary>
    /// Delete all objectives in the list and repopulate using active objectives
    /// </summary>
    public void UpdateObjectiveList()
    {
        if (_objectivesContentView.transform.childCount > 1)
        {
            for (int i = 1; i < _objectivesContentView.transform.childCount; i++)
            {
                Destroy(_objectivesContentView.transform.GetChild(i).gameObject);
            }
        }
        
        for (int i = 0; i < ObjectivesList.Count; i++)
        {
            if (ObjectivesList[i].Active == true)
            {
                GameObject objectiveListItem = Instantiate(_objectiveContentItemPrefab, _objectivesContentView.transform);
                objectiveListItem.GetComponentsInChildren<Text>()[0].text = ObjectivesList[i].Description;
                objectiveListItem.GetComponentsInChildren<Text>()[1].text = ObjectivesList[i].ProgressText;
            }
        }
    }

    public void ChangeButtonText(string newButtonText)
    {
        _objectivesButtonText.text = newButtonText;
    }
}

public class Objective
{
    bool _active;
    bool _complete;
    int _progressGoal;
    int _progressAmount;

    /// <summary>
    /// Create a new objective
    /// </summary>
    /// <param name="description">Objective description to display in objectives panel</param>
    /// <param name="active">Should quest be active</param>
    /// <param name="progressGoal">Progress goal or 0 if progress not required</param>
    public Objective(string description, bool active, int progressGoal)
    {
        Description = description;
        _active = active;
        _progressGoal = progressGoal;

        _complete = false;
        _progressAmount = 0;
    }

    public string Description { get; }

    public string ProgressText { get => (_progressGoal == 0)? "" : _progressAmount.ToString() + " / " + _progressGoal.ToString(); }
    public bool Complete { get { return _complete; } }
    public int CurrentProgress { get { return _progressAmount; } }
    public int ProgressGoal { set { _progressGoal = value; } }

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            ObjectiveMenuScript.instance.UpdateObjectiveList();
        }
    }

    public void UpdateProgress(int newProgressAmount)
    {
        _progressAmount = newProgressAmount;

        if (_progressAmount >= _progressGoal)
        {
            _complete = true;
            _active = false;
        }

        ObjectiveMenuScript.instance.UpdateObjectiveList();
    }
}

