using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class MouseSelectionController : MonoBehaviour
{
    Vector3 _mouseWorldLocation;
    Vector2 v2 = new Vector2();
    Vector2 v2f = new Vector2();
    [SerializeField]
    RectTransform SelectBox;
    Vector3 mousePosition1;
    Vector3 mousePosition2;
    List<SelectableObject> Selectables = new List<SelectableObject>();
    List<SelectionData> CurrentSelectionObjects = new List<SelectionData>();
    RaycastHit _hit;
    Ray _ray;
    enum SelectObjectFilter { Workers, All }

    [System.Serializable]
    struct SelectionData
    {
        public SelectableObject _Object;
        public Worker _Worker;
        public Building _Building;
        public RockScript _Rock;
        public Vehicle _Vehicle;
        public bool IsValid()
        {
            return _Object != null && _Object.IsSelectable();
        }
        public bool IsWorker()
        {
            return _Worker != null;
        }
        public bool IsBuilding()
        {
            return _Building != null;
        }
        public bool ShouldBeCulled()
        {
            return IsBuilding() || (_Rock != null);
        }

        public bool IsVehicle()
        {
            return _Vehicle != null;
        }
    }

    bool IsSelecting = false;
    bool DidJustFinishSelection = false;
    Camera MainCamera;
    public enum CurrentSelectionMode { Select, PlaceBuildings, PlaceOrders, RadialMenu, Breaker }

    CurrentSelectionMode Mode;
    UnitTask.TaskType CurrentBrush = UnitTask.TaskType.none;
    //Input
    bool LeftMouseDown = false;
    bool RightMouseDown = false;
    bool LeftUp = false;
    bool LeftMouseHold = false;
    bool QueueModDown = false;
    [SerializeField]
    float MinBoxSize = 2.0f;
    float CurrnetBoxSize = 0.0f;
    [ReadOnly, SerializeField]
    int SelectionCount = 0;
    Rect BrushDebug = new Rect(0, 250, 200, 100);
    bool ValidRayCast = false;
    [SerializeField]
    float MSSampleDistance = 4.0f;
    [SerializeField]
    int MaxWorkerVoices = 4;
    int CurrentOrderIndex = 0;
    [System.Serializable]
    struct CursourSet
    {
        public Texture2D Texture;
        public Vector2 HotSpot;
    }
    [Header("Mouse")]
    [SerializeField]
    CursourSet[] Cursors;
    public void SetMode(CurrentSelectionMode nmode)
    {
        if (Mode == nmode)
        {
            return;
        }
        if (Mode == CurrentSelectionMode.RadialMenu)
        {
            RadialMenuScript.instance.CloseMenu();
        }

        Mode = nmode;
    }
    public CurrentSelectionMode GetMode()
    {
        return Mode;
    }
    public static MouseSelectionController Get()
    {
        return WorldController.GetWorldController.MouseSelectionControl;
    }

    void Start()
    {
        MainCamera = WorldController.GetWorldController.MainCam;
        SelectBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!WorldController.IsRunning())
        {
            SelectBox.gameObject.SetActive(false);
            return;
        }
        DidJustFinishSelection = false;
        UpdateInput();
        ValidateCurrentSelection();
        UpdateOrderPaintMode();
        RaycastScene();
        if (IsSelecting)
        {
            DrawSelectBox();
            GatherObjects();
        }
        else
        {
            SelectBox.gameObject.SetActive(false);
        }
        TryToDecay();
        SelectionCount = CurrentSelectionObjects.Count;
        UpdateCursor();
    }

    void TryToDecay()
    {
        if (Mode == CurrentSelectionMode.RadialMenu)
        {
            if (!RadialMenuScript.instance.MenuOpen)
            {
                SetMode(CurrentSelectionMode.Select);
            }
            if (RadialMenuScript.instance.MenuOpen)
            {
                if (!EventSystem.current.IsPointerOverGameObject() && (LeftMouseDown))
                {
                    SetMode(CurrentSelectionMode.Select);
                }
            }
        }
    }

    bool CanSelect()
    {
        return !EventSystem.current.IsPointerOverGameObject() && Mode == CurrentSelectionMode.Select;
    }
    bool DoesSelectionContainWorkers()
    {
        foreach (SelectionData so in CurrentSelectionObjects)
        {
            if (so.IsWorker())
            {
                return true;
            }
        }
        return false;
    }
    void UpdateInput()
    {
        LeftMouseDown = ControlScript.instance.GetControl("Select").InputDown;
        RightMouseDown = ControlScript.instance.GetControl("Order").InputDown;
        LeftUp = ControlScript.instance.GetControl("Select").InputUp;
        LeftMouseHold = ControlScript.instance.GetControl("Select").AnyInput;
        QueueModDown = ControlScript.instance.GetControl("Order Modifier").AnyInput;
        if (CanSelect() || IsSelecting)
        {
            if (ControlScript.instance.GetControl("Select").InputDown)
            {
                StartSelection();
            }
            if (ControlScript.instance.GetControl("Select").InputUp)
            {
                EndSelection();
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))//todo: binds
        {
            SetMode(CurrentSelectionMode.Select);
            ClearSelectedObjects();
        }
    }

    bool HasSelection()
    {
        return CurrentSelectionObjects.Count > 0;
    }
    void ValidateCurrentSelection()
    {
        for (int i = CurrentSelectionObjects.Count - 1; i >= 0; i--)
        {
            if (!CurrentSelectionObjects[i].IsValid())
            {
                if (CurrentSelectionObjects[i]._Object)
                {
                    CurrentSelectionObjects[i]._Object.SetSelectionState(false);
                }
                CurrentSelectionObjects.RemoveAt(i);
            }
        }
    }
    void RaycastScene()
    {
        ValidRayCast = false;
        _ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out _hit))
        {
            ValidRayCast = true;
            Debug.DrawRay(_ray.origin, _ray.direction * _hit.distance, Color.yellow);
            _mouseWorldLocation = _hit.point;
            if (!IsOverUI())
            {
                if (Mode != CurrentSelectionMode.PlaceOrders)
                {
                    if (LeftUp && !DidJustFinishSelection)
                    {
                        IsSelecting = false;
                        ClearSelectedObjects();
                        if (_hit.transform.GetComponentInParent<Building>())
                        {
                            SelectObject(_hit.collider.gameObject);
                        }
                        else
                        {
                            SelectObject(_hit.collider.gameObject, SelectObjectFilter.Workers, true);
                        }
                        ShowWorkerListPanel();
                    }
                }
                if (RightMouseDown)
                {
                    if (HasSelection())
                    {
                        FindOrder();
                    }
                    else if (Mode == CurrentSelectionMode.Select || Mode == CurrentSelectionMode.RadialMenu)
                    {
                        SelectObject(_hit.collider.gameObject);
                        SelectUI();

                        ShowWorkerListPanel();
                    }

                }
                if (Mode == CurrentSelectionMode.Breaker)
                {
                    if (LeftMouseHold)
                    {
                        //break stuff!
                        BreakTile();
                    }
                }
            }
        }
    }

    bool IsOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void UpdateCursor()
    {
        if (Mode == CurrentSelectionMode.PlaceOrders)
        {
            Cursor.SetCursor(Cursors[0].Texture, Cursors[0].HotSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    void BreakTile()
    {
        if (IsOverUI())
        {
            return;
        }
        if (_hit.collider.gameObject != null)
        {
            Tile t = _hit.collider.gameObject.GetComponentInParent<Tile>();
            if (t != null)
            {
                WorldController.GetWorldController.GetComponent<TileMap3D>().ClearTile(t.X, t.Y);
            }
        }
    }

    void SelectUI()
    {
        if (!HasSelection())
        {
            return;
        }
        SelectionData Single = CurrentSelectionObjects[0];

        if (Single.IsWorker())
        {
            if (_hit.transform.tag == TagLibrary.HQ_TAG)
            {
                //RadialMenuScript.instance.ShowToolOptions(Single._Worker, _hit.transform.position);
                if (_hit.transform.GetComponent<Tile>() != null || _hit.transform.GetComponent<HQTileTagScript>() != null)
                {
                    ExecuteOrderOnAll(UnitTask.TaskType.Walk);
                }
            }
            else if (_hit.transform.tag == TagLibrary.VEHICLE_TAG)
            {
                if (!_hit.transform.GetComponent<Vehicle>().GetOccupied())
                {
                    RadialMenuScript.instance.ShowEmptyVehicleOptions(Single._Worker, _hit.transform.GetComponent<Vehicle>());
                }
            }
            else if (_hit.transform.tag == TagLibrary.WORKER_TAG)
            {
                Worker worker = _hit.transform.gameObject.GetComponentInParent<Worker>();
                List<Worker> workers = new List<Worker>();
                for (int i = 0; i < CurrentSelectionObjects.Count; i++)
                {
                    workers.Add(CurrentSelectionObjects[i]._Worker);
                }
                RadialMenuScript.instance.ShowWorkerOptions(workers, worker);
            }
            else
            {
                ExecuteOrderOnAll(UnitTask.TaskType.Walk);
            }
        }
        else
        {
            switch (_hit.transform.tag)
            {
                case TagLibrary.ROCK_TAG:
                    RockScript rock = _hit.transform.gameObject.GetComponentInParent<RockScript>();
                    RadialMenuScript.instance.ShowRockOptions(rock);
                    break;

                case TagLibrary.RUBBLE_TAG:
                    RubbleScript rubble = _hit.transform.gameObject.GetComponentInParent<RubbleScript>();
                    RadialMenuScript.instance.ShowRubbleOptions(rubble);
                    break;

                case TagLibrary.MONSTER_TAG:
                    Monster monster = _hit.transform.GetComponentInParent<Monster>();
                    RadialMenuScript.instance.ShowEnemyOptions(monster);
                    break;

                case TagLibrary.HQ_TAG:
                    HQ hq = _hit.transform.GetComponentInParent<HQ>();
                    RadialMenuScript.instance.ShowHQOptions(hq);
                    break;

                case TagLibrary.OUTPOST_TAG:
                    Outpost outpost = _hit.transform.GetComponentInParent<Outpost>();
                    RadialMenuScript.instance.ShowOutpostOptions(outpost);
                    break;

                case TagLibrary.GARAGE_TAG:
                    Garage garage = _hit.transform.GetComponentInParent<Garage>();
                    RadialMenuScript.instance.ShowGarageOptions(garage);
                    break;

                case TagLibrary.GEN_TAG:
                case TagLibrary.SKIP_TAG:
                case TagLibrary.BLOCK_TAG:
                case TagLibrary.TURRET_TAG:
                case TagLibrary.POWERGEN_TAG:
                    Building building = _hit.transform.GetComponentInParent<Building>();
                    RadialMenuScript.instance.ShowBuildingOptions(building);
                    break;

                case TagLibrary.ORE_TAG:
                    Ore ore = _hit.transform.GetComponentInParent<Ore>();
                    RadialMenuScript.instance.ShowResourceOptionsOre(ore);
                    break;

                case TagLibrary.ENERGYCRYSTAL_TAG:
                    EnergyCrystal energyCrystal = _hit.transform.GetComponentInParent<EnergyCrystal>();
                    RadialMenuScript.instance.ShowResourceOptionsEnergyCrystal(energyCrystal);
                    break;

                case TagLibrary.VEHICLE_TAG:
                    Vehicle vehicle = _hit.transform.GetComponent<Vehicle>();
                    if (vehicle.GetOccupied())
                    {
                        RadialMenuScript.instance.ShowVehicleOptions(_hit.transform.GetComponent<Vehicle>());
                    }
                    else
                    {
                        RadialMenuScript.instance.ShowEmptyVehicleOptions(null, _hit.transform.GetComponent<Vehicle>());
                    }
                    break;

                case TagLibrary.BURN_TAG:
                    MushroomCluster mushroomCluster = _hit.transform.GetComponentInParent<MushroomCluster>();
                    RadialMenuScript.instance.ShowMushroomOptions(mushroomCluster);
                    break;
            }
        }
        if (!Single.IsWorker())
        {
            ClearSelectedObjects(false);
        }
        SetMode(CurrentSelectionMode.RadialMenu);
    }

    void UpdateOrderPaintMode()
    {
        if (MenuScript.GamePaused)
        {
            return;
        }
        if (ControlScript.instance.GetControl(ControlScript.ORDERBRUSH_MINE).InputDown)
        {
            if (Mode == CurrentSelectionMode.PlaceOrders && CurrentBrush == UnitTask.TaskType.Mine)
            {
                CurrentBrush = UnitTask.TaskType.none;
                SetMode(CurrentSelectionMode.Select);
            }
            else
            {
                CurrentBrush = UnitTask.TaskType.Mine;
                SetMode(CurrentSelectionMode.PlaceOrders);
            }
        }
        if (ControlScript.instance.GetControl(ControlScript.ORDERBRUSH_CANCEL).InputDown)
        {
            if (Mode == CurrentSelectionMode.PlaceOrders && CurrentBrush == UnitTask.TaskType.none)
            {
                SetMode(CurrentSelectionMode.Select);
            }
            else
            {
                CurrentBrush = UnitTask.TaskType.none;
                SetMode(CurrentSelectionMode.PlaceOrders);
            }
        }
        if (Mode != CurrentSelectionMode.PlaceOrders)
        {
            return;
        }

        if (LeftMouseHold)
        {
            ApplyOrderToTargetTile();
        }
    }
    bool IsHitValid()
    {
        return ValidRayCast;
    }
    void ApplyOrderToTargetTile()
    {
        //prevent repeat!
        if (!IsHitValid())
        {
            return;
        }
        Tile t = null;
        try
        {
            t = _hit.transform.gameObject.GetComponentInParent<Tile>();
            if (t == null)
            {
                return;
            }
        }
        catch
        {
            return;
        }
        if (!CPU_FOW.Get().MultiSampleFOW(t.transform.position, MSSampleDistance))
        {
            return;
        }
        if (CurrentBrush == UnitTask.TaskType.Mine)
        {
            if (TryAddOrder(t, CurrentBrush))
            {
                TryAddOrder(t, UnitTask.TaskType.ClearRubble);
            }
        }
        else
        {
            TryAddOrder(t, CurrentBrush);
        }
    }
    bool TryAddOrder(Tile tile, UnitTask.TaskType type)
    {
        if (type == UnitTask.TaskType.none)
        {
            tile.CancelTilesTasks();
            return true;
        }
        UnitTask task = TaskLibrary.Get().CreateTask(type, tile.transform.position, tile.gameObject);
        if (task == null || !task.IsValid())
        {
            return false;
        }
        TaskList.AddTaskToGlobalTaskList(task);
        return true;
    }
    void FindOrder()
    {
        GameObject Target = _hit.collider.gameObject;
        if (CurrentSelectionObjects.Count > 0)
        {
            for (int i = 0; i < CurrentSelectionObjects.Count; i++)
            {
                if (CurrentSelectionObjects[i]._Object.gameObject == Target)
                {
                    SelectUI();
                    break;
                }
            }
        }
        switch (Target.tag)
        {
            case TagLibrary.FLOOR_TAG:
                ExecuteOrderOnAll(UnitTask.TaskType.Walk);
                return;
            case TagLibrary.ROCK_TAG:
                ExecuteOrderOnAll(UnitTask.TaskType.Mine);
                return;
            case TagLibrary.RUBBLE_TAG:
                ExecuteOrderOnAll(UnitTask.TaskType.ClearRubble);
                return;
            case TagLibrary.ORE_TAG:
            case TagLibrary.ENERGYCRYSTAL_TAG:
                ExecuteOrderOnAll(UnitTask.TaskType.Pickup);
                break;
            case TagLibrary.BURN_TAG:
                ExecuteOrderOnAll(UnitTask.TaskType.flameTarget);
                break;
        }
        Building B = Target.GetComponentInParent<Building>();
        if (B != null)
        {
            if (!B._Built)
            {
                ExecuteOrderOnAll(UnitTask.TaskType.Build);
            }
            else
            {
                switch (Target.tag)
                {
                    case TagLibrary.GEN_TAG:
                        if (DoesSelectionContainWorkers())
                        {
                            if (Target.GetComponentInParent<Building>()._Built && Target.GetComponentInParent<Building>().IsPowered)
                            {
                                ExecuteOrderOnAll(UnitTask.TaskType.RefillOxygen);
                                return;
                            }
                        }
                        else
                        {
                            SelectUI();
                            return;
                        }
                        break;
                    case TagLibrary.BLOCK_TAG:
                        ExecuteOrderOnAll(UnitTask.TaskType.Walk);
                        return;
                }
                SelectUI();
            }
        }
    }

    void ExecuteOrderOnAll(UnitTask.TaskType t)
    {
        ResetSounds(CurrentSelectionObjects.Count);
        foreach (SelectionData SD in CurrentSelectionObjects)
        {
            ExecuteOrder(t, SD);
            CurrentOrderIndex++;
        }
    }

    void ExecuteOrder(UnitTask.TaskType t, SelectionData SD)
    {
        if (SD.IsWorker())
        {
            WorkerExecute(t, SD);
        }
        else if (SD.IsVehicle())
        {
            VehicleExecute(t, SD);
        }
    }

    void WorkerExecute(UnitTask.TaskType t, SelectionData SD)
    {
        bool ShouldSet = !QueueModDown;
        if (t == UnitTask.TaskType.Walk)
        {
            AddTaskToWorker(t, SD._Worker, GetMousePosition(), null, ShouldSet);
            return;
        }
        else if (t == UnitTask.TaskType.ClearRubble)
        {
            if (!AddTaskToWorker(t, SD._Worker, _hit.point, _hit.collider.gameObject, ShouldSet))
            {
                AddTaskToWorker(UnitTask.TaskType.Walk, SD._Worker, GetMousePosition(), null, ShouldSet);
            }
            return;
        }
        AddTaskToWorker(t, SD._Worker, _hit.point, _hit.collider.gameObject, ShouldSet);
    }

    bool AddTaskToWorker(UnitTask.TaskType t, Worker W, Vector3 Pos, GameObject OBJ, bool set = false)
    {
        UnitTask newTask = TaskLibrary.Get().CreateTask(t, Pos, OBJ);
        if (newTask == null)
        {
            return false;
        }
        if (!TaskLibrary.CanWorkerExecuteTask(t, W))
        {
            return false;
        }
        if (set)
        {
            W.SetTask(newTask, ShouldPlaySound());
        }
        else
        {
            W.AddTask(newTask);
        }
        return true;
    }

    void VehicleExecute(UnitTask.TaskType t, SelectionData SD)
    {
        if (!SD.IsVehicle())
        {
            return;
        }
        bool ShouldSet = !QueueModDown;
        AddTaskToVehicle(t, SD._Vehicle, _hit.point, _hit.collider.gameObject, ShouldSet);
    }

    bool AddTaskToVehicle(UnitTask.TaskType t, Vehicle V, Vector3 Pos, GameObject OBJ, bool set = false)
    {
        UnitTask newTask = TaskLibrary.Get().CreateTask(t, Pos, OBJ);
        if (newTask == null)
        {
            return false;
        }
        if (!TaskLibrary.CanVehicleExecuteTask(t, V))
        {
            return false;
        }
        if (set)
        {
            V.SetTask(newTask);
        }
        else
        {
            V.AddTask(newTask);
        }
        return true;
    }

    void ClearSelectedObjects(bool CloseMenu = true)
    {
        foreach (SelectableObject SO in Selectables)
        {
            SO.SetSelectionState(false);
        }
        CurrentSelectionObjects.Clear();
        if (CloseMenu && Mode != CurrentSelectionMode.RadialMenu)
        {
            RadialMenuScript.instance.CloseMenu();
        }
    }

    void StartSelection()
    {
        ClearSelectedObjects();
        IsSelecting = true;
        mousePosition1 = Input.mousePosition;
    }

    void EndSelection()
    {
        GatherObjects(true);
        IsSelecting = false;
        if (CurrnetBoxSize > MinBoxSize)
        {
            DidJustFinishSelection = true;
        }

        ShowWorkerListPanel();
    }

    void GatherObjects(bool final = false)
    {
        mousePosition2 = Input.mousePosition;
        CurrnetBoxSize = Mathf.Max(Mathf.Abs(mousePosition1.x - mousePosition2.x), Mathf.Abs(mousePosition1.y - mousePosition2.y));
        if (CurrnetBoxSize < MinBoxSize)
        {
            return;
        }
        ResetSounds(CurrentSelectionObjects.Count);
        ClearSelectedObjects();
        foreach (SelectableObject SO in Selectables)
        {
            if (IsWithinSelectionBounds(SO.gameObject))
            {
                SelectObject(SO.gameObject, SelectObjectFilter.Workers, final);
                CurrentOrderIndex++;
            }
        }
    }

    void ResetSounds(int maxcount)
    {
        CurrentOrderIndex = 0;
    }

    bool ShouldPlaySound()
    {
        return CurrentOrderIndex < MaxWorkerVoices;
    }

    private void OnGUI()
    {
        //  GUI.Label(BrushDebug, "current Mode " + Mode.ToString() + "\nCurrent Brush Mode: " + CurrentBrush.ToString());
    }

    void SelectObject(GameObject GO, SelectObjectFilter Filter = SelectObjectFilter.All, bool final = false)
    {
        SelectionData SD = new SelectionData();
        SD._Object = GO.GetComponent<SelectableObject>();
        if (!SD.IsValid())
        {
            SD._Object = GO.GetComponentInParent<SelectableObject>();
            if (!SD.IsValid())
            {
                return;
            }
        }
        SD._Worker = GO.GetComponent<Worker>();
        SD._Vehicle = GO.GetComponent<Vehicle>();
        if (Filter == SelectObjectFilter.Workers)
        {
            if (!SD.IsWorker() && !SD.IsVehicle())
            {
                return;
            }
        }
        SD._Building = GO.GetComponentInParent<Building>();
        SD._Rock = GO.GetComponentInParent<RockScript>();
        if (SD.ShouldBeCulled())
        {
            if (!CPU_FOW.Get().MultiSampleFOW(GO.transform.position, MSSampleDistance))
            {
                return;
            }
        }
        SD._Object.SetSelectionState(true);
        if (final && SD.IsWorker() && ShouldPlaySound())
        {
            SD._Worker.PlaySelectionSound();
        }
        if (final && SD.IsVehicle() && ShouldPlaySound())
        {
            SD._Vehicle.PlaySelectionSound();
        }
        CurrentSelectionObjects.Add(SD);
    }

    public void DrawSelectBox()
    {
        SelectBox.gameObject.SetActive(true);
        Vector3 screenPosition1 = mousePosition1, screenPosition2 = Input.mousePosition;
        //    Debug.Log();
        screenPosition1.x = screenPosition1.x - (Screen.width / 2);
        screenPosition1.y = screenPosition1.y - (Screen.height / 2);

        v2.Set(screenPosition2.x - mousePosition1.x, mousePosition1.y - screenPosition2.y);

        if (v2.x < 0)
        {
            v2f.x = Mathf.Abs(v2.x);
            screenPosition1.x = screenPosition1.x - SelectBox.sizeDelta.x / 2;
        }
        else
        {
            v2f.x = v2.x;
            screenPosition1.x = screenPosition1.x + SelectBox.sizeDelta.x / 2;
        }
        if (v2.y < 0)
        {
            v2f.y = Mathf.Abs(v2.y);
            screenPosition1.y = screenPosition1.y + SelectBox.sizeDelta.y / 2;
        }
        else
        {
            v2f.y = v2.y;
            screenPosition1.y = screenPosition1.y - SelectBox.sizeDelta.y / 2;
        }
        SelectBox.localPosition = screenPosition1;
        SelectBox.sizeDelta = v2f;
    }

    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        if (!IsSelecting)
        {
            return false;
        }
        Bounds viewportBounds = Utils.Utils.GetViewportBounds(MainCamera, mousePosition1, mousePosition2);
        return viewportBounds.Contains(MainCamera.WorldToViewportPoint(gameObject.transform.position));
    }

    public Vector3 GetMousePosition()
    {
        return _mouseWorldLocation;
    }

    public void RegisterSelectable(SelectableObject SO)
    {
        if (Selectables.Contains(SO))
        {
            Debug.LogError("SelectableObject added twice " + SO.name);
            return;
        }
        Selectables.Add(SO);
    }

    public void UnRegisterSelectable(SelectableObject SO)
    {
        Selectables.Remove(SO);
        ValidateSelection();
    }

    void ShowWorkerListPanel()
    {
        List<Worker> workers = new List<Worker>();
        for (int i = 0; i < CurrentSelectionObjects.Count; i++)
        {
            if (CurrentSelectionObjects[i].IsWorker())
            {
                workers.Add(CurrentSelectionObjects[i]._Worker);
            }
        }

        WorkerInfoScript.instance.UpdateList(workers);
    }

    //Ensure all current selections are valid
    void ValidateSelection()
    {
        if (CurrentSelectionObjects.Count > 0)
        {
            for (int i = CurrentSelectionObjects.Count - 1; i >= 0; i--)
            {
                if (!CurrentSelectionObjects[i].IsValid())
                {
                    CurrentSelectionObjects.Remove(CurrentSelectionObjects[i]);
                }
            }
        }
    }
}
