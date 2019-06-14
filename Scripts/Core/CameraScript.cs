using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : IGridNotify
{
    readonly float _panBorderWidth = 50f;
    readonly float _UIPanWidth = 5.0f;
    [Header("Setup")]
    [SerializeField]
    Bounds CameraBounds = new Bounds(new Vector3(5, 5, 15), new Vector3(10, 10, 10));

    [Header("Zoom")]
    [SerializeField]
    float ZoomLerp = 0.5f;
    [SerializeField]
    Vector3 ClosePos = new Vector3(0, 1, 1);
    [SerializeField]
    Vector3 FarPos = new Vector3(0, 10, 10);
    [SerializeField]
    float RotateSpeed = 1.0f;
    [SerializeField]
    float ZoomSpeed = 1.0f;
    [SerializeField]
    float KeyZoomAmount = 0.1f;

    float newzoom = 0.0f;
    float CurrentZoom = 0.0f;
    Vector3 FinalPivotPos;
    Vector3 FinalCameraPos;
    GameObject CameraObject;
    GameObject CameraPivot;
    public bool CameraOnRails = false;
    Vector3 StartPos = new Vector3();
    [SerializeField]
    float GrabSenes = 1.0f;
    [Header("Snapping")]
    [SerializeField]
    float SnapAngle = 90;
    [SerializeField]
    float SnapLerpSpeed = 0.2f;
    float SnapTarget = 0.0f;
    float Padding = 50.0f;
    Vector3 Offset = new Vector3(4, 4, 0);
    void Start()
    {
#if UNITY_EDITOR
        SettingScript.instance.MousePanEnabled = false;
#endif       
        CameraObject = Camera.main.gameObject;
        CameraPivot = CameraObject.transform.parent.gameObject;
        ComputeBounds();
    }
    public void SetCameraPosAndRot(Vector3 pos, float YRot)
    {
        CameraPivot.transform.position = pos;
        SetRot(YRot);
    }
    void SetRot(float YRot)
    {
        Quaternion Q = new Quaternion();
        Q.eulerAngles = new Vector3(0, YRot, 0);
        CameraPivot.transform.rotation = Q;
    }
    public void SetPos(Vector3 pos)
    {
        pos.y = 0.0f;
        CameraPivot.transform.position = pos;
    }
    public Vector3 GetPos()
    {
        return CameraPivot.transform.position;
    }
    public void SetZoom(float zoom)
    {
        newzoom = zoom;
    }
    bool CanScroll()
    {
        return ScrollEnabled() && !EventSystem.current.IsPointerOverGameObject();
    }
    bool ScrollEnabled()
    {
        return SettingScript.instance.MousePanEnabled;
    }
    public static float Quantize(float value, float target)
    {
        return (Mathf.Round(value / target)) * target;
    }

    void RotateCamera(int direction, bool down = false)
    {
        if (down)
        {
            SnapTarget = CameraPivot.transform.rotation.eulerAngles.y;
            SnapTarget += SnapAngle * direction;
            SnapTarget = Quantize(SnapTarget, SnapAngle);
        }
        else
        {
            CameraPivot.transform.Rotate(Vector3.up, direction * RotateSpeed * Time.deltaTime);
            SnapTarget = CameraPivot.transform.rotation.eulerAngles.y;
        }
    }

    void Update()
    {
        ZoomSpeed = SettingScript.instance.CameraZoomSpeed;
        Vector3 Translation = Vector3.zero;
        if (!CameraOnRails && !MenuScript.GamePaused)
        {
            if (ControlScript.instance.GetControl("Camera Zoom In").AnyInput)
            {
                newzoom = CurrentZoom + -KeyZoomAmount;
            }
            if (ControlScript.instance.GetControl("Camera Zoom Out").AnyInput)
            {
                newzoom = CurrentZoom + KeyZoomAmount;
            }
            if (ControlScript.instance.GetControl(ControlScript.LEFTCONTROL).AnyInput)
            {
                if (ControlScript.instance.GetControl("Camera Snap Left").InputDown)
                {
                    RotateCamera(1, true);
                }
                if (ControlScript.instance.GetControl("Camera Snap Right").InputDown)
                {
                    RotateCamera(-1, true);
                }
                SetRot(Mathf.LerpAngle(CameraPivot.transform.rotation.eulerAngles.y, SnapTarget, SnapLerpSpeed));
            }
            else
            {
                if (ControlScript.instance.GetControl("Camera Snap Left").AnyInput)
                {
                    RotateCamera(1);
                }
                if (ControlScript.instance.GetControl("Camera Snap Right").AnyInput)
                {
                    RotateCamera(-1);
                }
            }

            if (SettingScript.instance.MouseZoomEnabled && Input.GetAxis("Mouse ScrollWheel") != 0 && !EventSystem.current.IsPointerOverGameObject())
            {
                float axis = Input.GetAxisRaw("Mouse ScrollWheel");
                newzoom = CurrentZoom + (-axis * ZoomSpeed);
            }

            if (ControlScript.instance.GetControl("Camera Rotate").InputDown)
            {
                StartPos = Input.mousePosition;
            }
            if (ControlScript.instance.GetControl(ControlScript.LEFTCONTROL).AnyInput)
            {
                if (ControlScript.instance.GetControl("Camera Rotate").AnyInput)
                {
                    Vector3 delta = (StartPos - Input.mousePosition) * SettingScript.instance.CameraPanSpeed * GrabSenes;
                    Translation.z = delta.y;
                    Translation.x = delta.x;
                    StartPos = Input.mousePosition;
                }
            }
            else
            {
                if (ControlScript.instance.GetControl("Camera Rotate").AnyInput)
                {
                    Vector3 delta = (StartPos - Input.mousePosition);
                    CameraPivot.transform.Rotate(Vector3.up, -delta.x * SettingScript.instance.CameraRotSpeed * Time.deltaTime);
                    StartPos = Input.mousePosition;
                }
            }
            if (ControlScript.instance.GetControl("Camera Forward").AnyInput || (CanScroll() && Input.mousePosition.y >= Screen.height - _panBorderWidth) || (ScrollEnabled() && Input.mousePosition.y >= Screen.height - _UIPanWidth))
            {
                Translation.z += SettingScript.instance.CameraPanSpeed * Time.deltaTime;
            }
            if (ControlScript.instance.GetControl("Camera Backward").AnyInput || (CanScroll() && Input.mousePosition.y <= _panBorderWidth) || (ScrollEnabled() && Input.mousePosition.y <= _UIPanWidth))
            {
                Translation.z -= SettingScript.instance.CameraPanSpeed * Time.deltaTime;
            }
            if (ControlScript.instance.GetControl("Camera Left").AnyInput || (CanScroll() && Input.mousePosition.x <= _panBorderWidth) || (ScrollEnabled() && Input.mousePosition.x <= _UIPanWidth))
            {
                Translation.x -= SettingScript.instance.CameraPanSpeed * Time.deltaTime;
            }
            if (ControlScript.instance.GetControl("Camera Right").AnyInput || (CanScroll() && Input.mousePosition.x >= Screen.width - _panBorderWidth) || (ScrollEnabled() && Input.mousePosition.x >= Screen.width - _UIPanWidth))
            {
                Translation.x += SettingScript.instance.CameraPanSpeed * Time.deltaTime;
            }
        }
        CurrentZoom = Mathf.Lerp(CurrentZoom, newzoom, ZoomLerp);
        FinalCameraPos = CameraObject.transform.localPosition;
        CurrentZoom = Mathf.Clamp(CurrentZoom, 0.0f, 1.0f);
        FinalCameraPos = Vector3.Lerp(ClosePos, FarPos, CurrentZoom);
        CameraObject.transform.localPosition = FinalCameraPos;

        FinalPivotPos = CameraPivot.transform.position;
        FinalPivotPos.y = 0.0f;
        //apply the translation
        FinalPivotPos += AllignPosition(Translation);
        FinalPivotPos = ConstrainPosition(FinalPivotPos);
        CameraPivot.transform.position = FinalPivotPos;
        CameraObject.transform.LookAt(CameraPivot.transform);
    }

    /// <summary>
    /// Align the camera position with the camera's direction
    /// </summary>
    /// <param name="Pos">Desired camera position</param>
    /// <returns>New camera position</returns>
    Vector3 AllignPosition(Vector3 Pos)
    {
        Vector3 newPos = new Vector3(CameraObject.transform.forward.x, 0, CameraObject.transform.forward.z).normalized * Pos.z;
        newPos += new Vector3(CameraObject.transform.right.x, 0, CameraObject.transform.right.z).normalized * Pos.x;
        newPos += new Vector3(0, CameraObject.transform.up.y, 0).normalized * Pos.y;
        return newPos;
    }

    /// <summary>
    /// Constrain camera position within specified bounds
    /// </summary>
    /// <param name="Pos">Desired camera position</param>
    /// <returns>New camera position</returns>
    Vector3 ConstrainPosition(Vector3 Pos)
    {
        Pos.x = Mathf.Clamp(Pos.x, CameraBounds.min.x, CameraBounds.max.x);
        Pos.z = Mathf.Clamp(Pos.z, CameraBounds.min.y, CameraBounds.max.y);
        return Pos;
    }

    /// <summary>
    /// Set camera bounds using grid size
    /// </summary>
    public void ComputeBounds()
    {
        TileMap3D Tilemap = GetComponent<TileMap3D>();
        if (Tilemap == null)
        {
            float MaxValue = 10e9f;
            CameraBounds = new Bounds(Vector3.zero, new Vector3(MaxValue, MaxValue, MaxValue));
            return;
        }
        float XSize = Tilemap.GridXSize_ * Tilemap.GridSpacing;
        float YSize = Tilemap.GridYSize_ * Tilemap.GridSpacing;

        Vector3 pos = Tilemap.GetGridCentre();
        pos.y = pos.z;
        pos.z = 0.0f;

        CameraBounds = new Bounds(Offset + pos, new Vector3(XSize - Padding, YSize - Padding, 20));
    }

    /// <summary>
    /// Draw gizmos for camera bounds
    /// </summary>
    void RenderBounds()
    {
        const float y = 10;
        Gizmos.DrawLine(new Vector3(CameraBounds.max.x, y, CameraBounds.min.y), new Vector3(CameraBounds.max.x, y, CameraBounds.max.y));
        Gizmos.DrawLine(new Vector3(CameraBounds.min.x, y, CameraBounds.min.y), new Vector3(CameraBounds.max.x, y, CameraBounds.min.y));
        Gizmos.DrawLine(new Vector3(CameraBounds.min.x, y, CameraBounds.max.y), new Vector3(CameraBounds.min.x, y, CameraBounds.min.y));
        Gizmos.DrawLine(new Vector3(CameraBounds.max.x, y, CameraBounds.max.y), new Vector3(CameraBounds.min.x, y, CameraBounds.max.y));
        if (CameraPivot != null)
        {
            Gizmos.DrawSphere(CameraPivot.transform.position, 1.0f);
        }
        Vector3 pos = CameraBounds.center;
        pos.z = pos.y;
        pos.y = 20.0f;
        Gizmos.DrawSphere(pos, 1.0f);
    }

    void OnDrawGizmos()
    {
        RenderBounds();
    }

    public override void OnGridCreationFinished()
    {
        ComputeBounds();
    }

}