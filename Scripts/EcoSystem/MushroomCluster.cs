using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomCluster : MonoBehaviour
{
    [SerializeField] List<GameObject> _singleMushrooms = new List<GameObject>();
    [SerializeField] GameObject _slug;
    List<GameObject> _spawnedMushrooms = new List<GameObject>();
    GameObject _recentMushroom;
    float _recentMushroomSize;
    TileMap3D _map;
    Tile _attachedTile;
    int _count = 0;
    WorldController _worldContoller;
    float _spawnTime;
    float DefaultGrowSpeed = 0.0f;
    [SerializeField]
    float burnSpeed = 0.1f;
    float _speadTime;
    public void ResetSpawnTimer()
    {
        _spawnTime = Random.Range(60, 120);
        
        if (WorldController.GetWorldController.DEBUG_FAST_GROW)
        {
            _spawnTime = 1.0f;
            _worldContoller.GrowSpeed = 0.5f;
        }
        else
        {
            _worldContoller.GrowSpeed = DefaultGrowSpeed;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        _speadTime = Random.Range(30, 60);
        _worldContoller = WorldController.GetWorldController;
        DefaultGrowSpeed = _worldContoller.GrowSpeed;
        ResetSpawnTimer();
        _map = StaticMapInfo.Map;
        Tile thisTile = _map.FindCell(transform.position);
        if (thisTile.tag == "Floor")
        {
            transform.parent = thisTile.transform;
            _attachedTile = thisTile;
            _worldContoller._mushroomClusters.Add(this);
            TaskLibrary.Get().CreateTask(UnitTask.TaskType.flameTarget, transform.position, gameObject);
        }
        else
        {
            Object.Destroy(this);
        }

    }

    public void FirstMushRoom(float x, float y, float z)
    {
        SpawnMushroom(x, y, z);
    }

    void SpawnMushroom(float x, float y, float z)
    {
        _recentMushroom = Instantiate(_singleMushrooms[Random.Range(0, _singleMushrooms.Count)], new Vector3(x, y, z), new Quaternion(0, 0, 0, 0));
        _recentMushroomSize = Random.Range(0.5f, 1.5f);
        _recentMushroom.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
        _recentMushroom.transform.parent = transform;
        _recentMushroom.transform.localScale = new Vector3(0, 0, 0);
        _spawnedMushrooms.Add(_recentMushroom);
    }

    void NewMushroom()
    {

        bool goodLocation = false;
        Vector3 newMushroomLocation;
        int loop = 0;
        do
        {
            loop++;
            if (loop >= 10)
            {

                return;
            }

            int chosenMushroom = Random.Range(0, _spawnedMushrooms.Count);

            Vector3 currentLocation = _spawnedMushrooms[chosenMushroom].transform.position;
            float localscaleX = _spawnedMushrooms[chosenMushroom].transform.lossyScale.x;
            float localscaleY = _spawnedMushrooms[chosenMushroom].transform.lossyScale.x;

            if ((Random.Range(0, 2) == 0))
            {
                localscaleX = Random.Range(0, Random.Range(0, localscaleX));
            }
            else
            {
                localscaleY = Random.Range(0, Random.Range(0, localscaleY));
            }

            if (Random.Range(0, 2) == 0)
            {
                localscaleX *= -1;
            }
            if (Random.Range(0, 2) == 0)
            {
                localscaleY *= -1;
            }



            newMushroomLocation = new Vector3(currentLocation.x + localscaleX, currentLocation.y, currentLocation.z + localscaleY);
            bool inMeshBounds = false;
            for (int i = 0; i < _spawnedMushrooms.Count && !inMeshBounds; i++)
            {
                BoxCollider boxCollider = _spawnedMushrooms[i].GetComponentInChildren<BoxCollider>();
                if (boxCollider != null)
                {
                    inMeshBounds = boxCollider.bounds.Contains(newMushroomLocation);

                }
            }
            if (!inMeshBounds)
            {
                Tile newTile = _map.FindCell(newMushroomLocation);
                if (newTile.X == _attachedTile.X && newTile.Y == _attachedTile.Y)
                {
                    goodLocation = true;
                }
                else
                {

                    if (newTile.tag == "Floor" && newTile.GetComponentInChildren<MushroomCluster>() == null)
                    {

                        Vector3 newLocation = newTile.transform.position;
                        GameObject newCluster = Instantiate(StaticMapInfo.RockModleHolder.MushroomCluster, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));

                        if (newCluster != null)
                        {
                            newCluster.GetComponent<MushroomCluster>().FirstMushRoom(newMushroomLocation.x, newMushroomLocation.y, newMushroomLocation.z);
                        }
                    }
                }
            }
        }
        while (!goodLocation);

        SpawnMushroom(newMushroomLocation.x, newMushroomLocation.y, newMushroomLocation.z);

    }

    bool Spread(Tile tile, float xPlus, float zPlus)
    {

        if (tile.tag == "Floor" && tile.GetComponentInChildren<MushroomCluster>() == null)
        {

            Vector3 newLocation = tile.transform.position;
            GameObject newCluster = Instantiate(StaticMapInfo.RockModleHolder.MushroomCluster, new Vector3(newLocation.x, newLocation.y, newLocation.z), new Quaternion(0, 0, 0, 0));

            if (newCluster != null)
            {
                newCluster.GetComponent<MushroomCluster>().FirstMushRoom(transform.position.x + xPlus, transform.position.y, transform.position.z + zPlus);
                return true;
            }
  
        }
        return false;

    }
 
    [SerializeField]
    int MushroomsPerTile = 10;
    
    // Update is called once per frame
    void Update()
    {
        if (_count <= MushroomsPerTile)
        {
            if (_recentMushroom)
            {
                _recentMushroom.transform.localScale += new Vector3(_worldContoller.GrowSpeed * _recentMushroomSize, _worldContoller.GrowSpeed * _recentMushroomSize, _worldContoller.GrowSpeed * _recentMushroomSize) * Time.deltaTime;
                if (_recentMushroom.transform.localScale.x >= _recentMushroomSize)
                {
                    _count++;
                    NewMushroom();
                }
            }
        }
        else
        {
            _speadTime -= Time.deltaTime;
            if (_speadTime <= 0)
            {
                _speadTime = Random.Range(30, 60);

                int x = _attachedTile.X;
                int y = _attachedTile.Y;

                int speadTo = Random.Range(0, 8);
                bool canSpead = false;
                int count = 0;
                do
                {
                    switch (speadTo)
                    {
                        case 0:
                            canSpead = Spread(_map.GetTileAtPos(x + 1, y), 5, 0);
                            break;
                        case 1:
                            canSpead = Spread(_map.GetTileAtPos(x + 1, y + 1), 5, 5);
                            break;
                        case 2:
                            canSpead = Spread(_map.GetTileAtPos(x, y + 1), 0, 5);
                            break;
                        case 3:
                            canSpead = Spread(_map.GetTileAtPos(x - 1, y + 1), -5, 5);
                            break;
                        case 4:
                            canSpead = Spread(_map.GetTileAtPos(x - 1, y), -5, 0);
                            break;
                        case 5:
                            canSpead = Spread(_map.GetTileAtPos(x - 1, y - 1), -5, -5);
                            break;
                        case 6:
                            canSpead = Spread(_map.GetTileAtPos(x, y - 1), 0, -5);
                            break;
                        case 7:
                            canSpead = Spread(_map.GetTileAtPos(x + 1, y - 1), 5, -5);
                            break;
                    }
                    count++;
                    if(!canSpead)
                    {
                        speadTo++;
                        if(speadTo <= 8)
                        {
                            speadTo = 0;
                        }
                    }
                }
                while (!canSpead && _count <= 8);
            }

        }

        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0)
        {
            ResetSpawnTimer();
            if (_worldContoller._monsters.Count < 6)
            {
                for (int i = 0; i < _count / (MushroomsPerTile/ 2); i++)
                {
                    Instantiate(_slug, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
                }
            }
           
        }
    }
    public void Burn(GameObject flame)
    {
        BoxCollider boxCollider = flame.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            for (int i = _spawnedMushrooms.Count - 1; i >= 0; i--)
            {
                if (boxCollider.bounds.Contains(_spawnedMushrooms[i].transform.position))
                {
                    _spawnedMushrooms[i].transform.localScale -= new Vector3(burnSpeed * _recentMushroomSize, burnSpeed * _recentMushroomSize, burnSpeed * _recentMushroomSize) * Time.deltaTime;
                    if (_spawnedMushrooms[i].transform.localScale.x <= 0.1f)
                    {
                        GameObject burnedMushroom = _spawnedMushrooms[i];
                        _spawnedMushrooms.Remove(burnedMushroom);
                        Object.Destroy(burnedMushroom);
                        _worldContoller._levelStatsController.MushroomBurned();
                        _count--;
                    }
                }
            }
            if (_spawnedMushrooms.Count == 0)
            {
                
                Object.Destroy(this);
            }
        }
    }

    private void OnDestroy ()
    {
        _worldContoller._mushroomClusters.Remove(this);
    }

}
