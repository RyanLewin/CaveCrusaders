using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Camera _mainCamera, _miniMapCamera;
    //public Button _enlargeButton;
    public LineRenderer _cameraView;
    public GameObject _enlargedMiniMap, _miniMap;

    bool _enlarged;
    Vector3[] _cameraCorners = new Vector3[4];
    //Vector3 _cameraCentre, _translation, _corner;
    Vector3 _translation, _corner;
    Button _mapButton;
    TileMap3D _currentLevel;
    Bounds _bounds;
    float _orthagraphicSize;
    const float Padding = 10.0f;

    Ray _ray;
    RaycastHit _hit;

    void Start()
    {
        //initialising our variables
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _mapButton = _miniMap.GetComponentInChildren<Button>();
        _mapButton.onClick.AddListener(MoveCamera);
        //_enlargeButton.onClick.AddListener(EnlargeMiniMap);

        _enlarged = false;
        //_cameraCentre = new Vector3();
        _translation = new Vector3();
    }

    private void LateUpdate()
    {
        //Grabbing the tilemap in LateUpdate to ensure the correct tilemap id grabbed and loaded, then setting the minimaps position
        _currentLevel = GameObject.FindGameObjectWithTag("Logic").GetComponent<TileMap3D>();
        SetMinimapPosition(CalculateBounds());
    }

    void Update()
    {
        DrawCameraView();
    }

    /// <summary>
    /// Uses line renderer to draw the user's viewport to the minimap
    /// </summary>
    void DrawCameraView()
    {
        //storing the vectors that represent the corners of the camera
        _cameraCorners[0] = new Vector3(0, 0, 0);
        _cameraCorners[1] = new Vector3(0, 1, 0);
        _cameraCorners[2] = new Vector3(1, 1, 0);
        _cameraCorners[3] = new Vector3(1, 0, 0);

        for (int i = 0; i < _cameraCorners.Length; i++)
        {
            //turning our vectors into rays from the corners of the camera
            _ray = _mainCamera.ViewportPointToRay(_cameraCorners[i]);

            //setting the distance to render the main camera's view point at
            int distance = 80;

            if (i == 0 || i == 3)
                distance = 50;

            //increasing the y value of those points to render above the minimap icons
            _cameraCorners[i] = new Vector3(_ray.GetPoint(distance).x, _ray.GetPoint(distance).y + 200, _ray.GetPoint(distance).z);
        }

        //passes the four corners of the player's view to the line renderer to be drawn on the minimap
        _cameraView.positionCount = _cameraCorners.Length;
        _cameraView.SetPositions(_cameraCorners);
    }

    /// <summary>
    /// Enlarges the minimap to allow the user to see the whole map
    /// </summary>
    public void EnlargeMiniMap()
    {
        // if the minimap isn't already enlarged, then enlarge it. Deactivate the small minimap aswell.
        if (!_enlarged)
        {
            _enlarged = true;

            _enlargedMiniMap.SetActive(true);
            _miniMap.gameObject.SetActive(false);
        }
        else
        {
            //if however we already enlarged the minimap, then turn off the enlarged minimap and activate the smaller one
            _enlarged = false;

            _enlargedMiniMap.SetActive(false);
            _miniMap.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Allign the camera position with the location of the users click on the minimap in worldspace
    /// </summary>
    void MoveCamera()
    {
        //gets the centre of the users view as a world coordinate, works out the distance between that and the new point the user chooses, then apply that equivalent transformation to the camera
        Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out _hit, Mathf.Infinity);
        //todo: this needs an offset
        _translation = _hit.point - _miniMapCamera.ScreenToWorldPoint(Input.mousePosition);
        CameraScript c = WorldController.GetWorldController.GetComponent<CameraScript>();
        c.SetPos(c.GetPos() - new Vector3(_translation.x, 0, _translation.z));
    }

    /// <summary>
    /// Allign the minimap camera to be centered on the level and encompass the tilemap
    /// </summary>
    /// <param name="currentLevel"> An instance of the current tilemap used in the level being played</param>
    void SetMinimapPosition(Bounds bounds)
    {
        //set the camera to the centre of the grid
        _miniMapCamera.transform.position = _currentLevel.GetGridCentre();

        //depending on if one side is bigger or smaller, expand our view to see the largest of these sides
        if (bounds.size.x >= bounds.size.y)
        {
            _orthagraphicSize = (bounds.size.x / 2) + Padding;
        }
        else
        {
            _orthagraphicSize = (bounds.size.y / 2) + Padding;
        }

        //set the final position and size of the camera
        _miniMapCamera.transform.position = new Vector3(bounds.center.x, 300, bounds.center.y);
        _miniMapCamera.orthographicSize = _orthagraphicSize;
    }

    /// <summary>
    /// Calculate the bounds of  the current level
    /// </summary>
    /// <returns> A bounding box of the level </returns>
    Bounds CalculateBounds()
    {
        //calculate the bounds of the level and return them
        float XSize = _currentLevel.GridXSize_ * _currentLevel.GridSpacing - Padding;
        float YSize = _currentLevel.GridYSize_ * _currentLevel.GridSpacing - Padding;

        _bounds = new Bounds(new Vector3(XSize / 2, YSize / 2, 20), new Vector3(XSize, YSize, 20));

        return _bounds;
    }
}
