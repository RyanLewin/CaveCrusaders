using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
public class MapSaveController : MonoBehaviour
{
    public struct MapRefrence
    {
        public string name;
        public bool IsResource;
        public MapRefrence(string filename, bool res = false)
        {
            name = filename;
            IsResource = res;
        }
    }
    FileHeader FileData;
    [SerializeField]
    public TileMap3D _TileMap = null;
    [SerializeField]
    public TileLibrary Lib = null;
    [HideInInspector]
    public string CurrentFileName = "";
    string RootPath = "";
    string EditorRootPath = "";
    AG_Profiler LoadSampler;
    bool LoadOnStart = false;
    bool LoadResource = false;
    const string ResourcePath = "TileSets/";
    public void SetLevelName(string name, bool IsResource)
    {
        if (name == null || name.Length == 0)
        {
            return;
        }
        CurrentFileName = name;
        LoadOnStart = true;
        LoadResource = IsResource;
    }
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        LoadSampler = new AG_Profiler();
#endif
        SetLevelName(StaticMapInfo.Level, StaticMapInfo.IsLevelInBuilt);
        FileData = new FileHeader();
        //todo: read from resource file in package
        RootPath = Application.streamingAssetsPath + "/TileSets/";
        EditorRootPath = Application.dataPath + "/Resources/TileSets/";
        Directory.CreateDirectory(RootPath);
        if (LoadOnStart)
        {
            LoadTileSet(CurrentFileName);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<MapRefrence> GetSavedMapNames()
    {
        List<MapRefrence> mapNames = new List<MapRefrence>();

#if UNITY_EDITOR
        DirectoryInfo devDirectory = new DirectoryInfo(EditorRootPath);

        foreach (FileInfo fileInfo in devDirectory.GetFiles("*.JSON"))
        {
            mapNames.Add(new MapRefrence(fileInfo.Name.Replace(".JSON", ""), true));
        }
#else
        var objects = Resources.LoadAll(ResourcePath);
        foreach (object o in objects)
        {
            try
            {
                TextAsset t = (TextAsset)o;
                if (t != null)
                {
                    mapNames.Add(new MapRefrence(t.name, true));
                }
            }
            catch
            {

            }
        }
#endif

        DirectoryInfo directory = new DirectoryInfo(RootPath);

        foreach (FileInfo fileInfo in directory.GetFiles("*.JSON"))
        {
            mapNames.Add(new MapRefrence(fileInfo.Name.Replace(".JSON", "")));
        }

        return mapNames;
    }

    bool GetFileData(string Filepath, bool ISRes, out string filedata)
    {
        filedata = "";

        var jsonTextFile = Resources.Load<TextAsset>(ResourcePath + Filepath);
        if (jsonTextFile != null)
        {
            filedata = jsonTextFile.text;
            return true;
        }
        ISRes = false;
        Debug.Log("Failed to find as resource");
        //attempt to find as loose file
        string Path = RootPath + Filepath + ".JSON";
        try
        {
            StreamReader streamReader = File.OpenText(Path);
            if (streamReader != null)
            {
                filedata = streamReader.ReadToEnd();
                return true;
            }
        }
        catch
        {
            Debug.Log("Failed to find map " + Path);
        }
        return false;
    }

    public void DeleteTileSet(string Filepath)
    {
        DirectoryInfo directory;

        if (LevelEditor.IsDeveloperMode())
        {
            directory = new DirectoryInfo(EditorRootPath);
        }
        else
        {
            directory = new DirectoryInfo(RootPath);
        }

        foreach (FileInfo fileInfo in directory.GetFiles("*.JSON"))
        {
            if (fileInfo.Name.Replace(".JSON", "") == Filepath)
            {
                fileInfo.Delete();
                return;
            }
        }
    }

    public bool IsFileResource(string Filepath)
    {
        DirectoryInfo directory = new DirectoryInfo(EditorRootPath);
        foreach (FileInfo fileInfo in directory.GetFiles("*.JSON"))
        {
            if (fileInfo.Name.Replace(".JSON", "") == Filepath)
            {
                return true;
            }
        }

        return false;
    }
    public void Editor_LoadTileSet(string Filepath, bool IsRes)
    {
        LoadResource = IsRes;
        LoadTileSet(Filepath);
    }
    public void LoadTileSet(string Filepath)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (LoadSampler != null)
        {
            LoadSampler.Begin();
        }
#endif
        string jsonString = "";
        if (GetFileData(Filepath, LoadResource, out jsonString))
        {
            FileData = JsonUtility.FromJson<FileHeader>(jsonString);
            BuildGridWithSavedData();
            RockProximty();
            Debug.Log("Loaded TileSet from " + Filepath);
        }
        else
        {
            Debug.LogError("Failed to load TileSet from " + Filepath);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (LoadSampler != null)
        {
            LoadSampler.End();
            Debug.Log("Map load took " + LoadSampler.GetMS() + "MS ");
        }
#endif

    }

    GameObject GetTile(TileEntry entr)
    {
        return Lib.GetTilePrefab(entr.ID);
    }

    void BuildGridWithSavedData()
    {
        _TileMap.NotifyLoading(true);
        _TileMap.DestroyGrid();
        _TileMap.SetGridSize(FileData.GridXSize, FileData.GridYSize);
        _TileMap.BuildTiles();
        int Itor = 0;
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                _TileMap.UpdateTile(x, y, GetTile(FileData.Data[Itor]), true);
                Tile t = _TileMap.GetTileAtPos(x, y);
                FileData.Data[Itor].ApplySeralToTile(t);
                if (t.IgnoreBuild)
                {
                    int tId = t.GetID();
                    string tTag = t.tag;
                    if (t.GetComponent<Building>())
                    {
                        Vector2[] temp = t.GetComponent<Building>()._BuildingSize;
                        if (temp.Length > 0)
                        {
                            for (int i = 0; i < temp.Length; i++)
                            {
                                Tile OtherT = _TileMap.GetTileAtPos(x + (int)temp[i].x, y + (int)temp[i].y);
                                _TileMap.UpdateTile(OtherT.X, OtherT.Y, TileLibrary.Get().GetTilePrefab(OtherT.GetOldID()));
                            }
                        }
                    }
                    _TileMap.ClearTile(x, y);
                    t = _TileMap.GetTileAtPos(x, y);
                    t.SetID(tId);
                    t.tag = tTag;
                    t.IgnoreBuild = true;
                }
                t.TileStart();
                //UpdateBuilding(t);
                Itor++;
            }
        }
        _TileMap.BroadcastGridCreate();
        _TileMap.NotifyLoading(false);
    }

    void UpdateBuilding(Tile t)
    {
        if (t.GetComponent<Building>())
        {
            Building building = t.GetComponent<Building>();
            building.CheckIfBuilt();
        }
    }

    void BuildDataForSave()
    {
        int SizeTotal = _TileMap.GridXSize_ * _TileMap.GridYSize_;
        FileData.Data = new TileEntry[SizeTotal];
        FileData.GridXSize = _TileMap.GridXSize_;
        FileData.GridYSize = _TileMap.GridYSize_;
        int Itor = 0;
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                FileData.Data[Itor] = new TileEntry();
                FileData.Data[Itor].BuildForTile(_TileMap.GetTileAtPos(x, y));
                Itor++;
            }
        }
    }

    public void SaveTileSet(string filepath, bool SaveToResource)
    {
        string Path = RootPath;/// RootPath + filepath + ".JSON";
        if (SaveToResource)
        {
#if UNITY_EDITOR
            Path = EditorRootPath;
#else
            Debug.LogError("Saving to resources is only supported in the editor");
#endif
        }
        Path += filepath + ".JSON";
        BuildDataForSave();

        string jsonString = JsonUtility.ToJson(FileData);

        using (StreamWriter streamWriter = File.CreateText(Path))
        {
            streamWriter.Write(jsonString);
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("Saved TileSet to " + Path);
    }
    void RockProximty()
    {
        float navMeshSize = 5;
        float tilesize = StaticMapInfo.RockModleHolder.DefultTile.GetComponent<MeshRenderer>().bounds.size.x / 2;
        WorldController worldController = WorldController.GetWorldController;
        List<GameObject> ambientObjects = StaticMapInfo.RockModleHolder.AmbientObjects;
        List<Vector3> oldVector3 = new List<Vector3>();
        List<float> oldSize = new List<float>();
        worldController._landslideRocks.Clear();
        worldController._defultTile.Clear();
        for (int x = 0; x < _TileMap.GridXSize_; x++)
        {
            for (int y = 0; y < _TileMap.GridYSize_; y++)
            {
                Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y);
                if (tile.tag == "RockTile")
                {
                    RockScript tileRockSript = tile.GetComponent<RockScript>();
                    if (tileRockSript.RockType != RockScript.Type.SolidRock)
                    {
                        worldController._landslideRocks.Add(tileRockSript);
                    }

                    tileRockSript.RockNorth = StaticMapInfo.isWall(x, y + 1);
                    tileRockSript.RockNorthEast = StaticMapInfo.isWall(x + 1, y + 1);
                    tileRockSript.RockEast = StaticMapInfo.isWall(x + 1, y);
                    tileRockSript.RockSouthEast = StaticMapInfo.isWall(x + 1, y - 1);
                    tileRockSript.RockSouth = StaticMapInfo.isWall(x, y - 1);
                    tileRockSript.RockSouthWest = StaticMapInfo.isWall(x - 1, y - 1);
                    tileRockSript.RockWest = StaticMapInfo.isWall(x - 1, y);
                    tileRockSript.RockNorthWest = StaticMapInfo.isWall(x - 1, y + 1);
                }
                else if (tile.tag == "Floor")
                {
                    worldController._defultTile.Add(tile);
                    int ambinetObjectsAmount = 0;

                    if (!(StaticMapInfo.isWall(x, y + 1) ||
                    StaticMapInfo.isWall(x + 1, y + 1) ||
                    StaticMapInfo.isWall(x + 1, y) ||
                    StaticMapInfo.isWall(x + 1, y - 1) ||
                    StaticMapInfo.isWall(x, y - 1) ||
                    StaticMapInfo.isWall(x - 1, y - 1) ||
                    StaticMapInfo.isWall(x - 1, y) ||
                    StaticMapInfo.isWall(x - 1, y + 1)))
                    {
                        ambinetObjectsAmount = 3;
                    }
       
                    if (!StaticMapInfo.IsLevelInBuilt && WorldController.IsPlaying())
                    {
                        GameObject ranAmbentObject = ambientObjects[UnityEngine.Random.Range(0, ambientObjects.Count)];
                        if (ranAmbentObject != null)
                        {
                            if (ranAmbentObject.tag == "LightAmbent")
                            {
                                float scale = UnityEngine.Random.Range(0.4f, 1.1f);
                                ranAmbentObject.transform.localScale = new Vector3(scale, scale, scale);
                            }
                            MeshRenderer[] objectMeshRenders = ranAmbentObject.GetComponentsInChildren<MeshRenderer>();

                            float ranAmbentObjectSize = 0;

                            for (int i = 0; i < objectMeshRenders.Length; i++)
                            {
                                Vector3 objectSize = objectMeshRenders[i].bounds.size;
                                if (objectSize.x > ranAmbentObjectSize || objectSize.z > ranAmbentObjectSize)
                                {
                                    ranAmbentObjectSize = Mathf.Max(objectSize.x, objectSize.z);
                                }
                            }

                            if (ranAmbentObjectSize != 0)
                            {
                                ranAmbentObjectSize /= 2;
                                for (int i = 0; i < ambinetObjectsAmount; i++)
                                {
                                    bool foundPos = false;
                                    int reposisionTimes = 0;
                                    Vector3 vector3;

                                    do
                                    {
                                        float tileOutlier = tilesize - ranAmbentObjectSize;
                                        vector3 = new Vector3(UnityEngine.Random.Range(-tileOutlier, tileOutlier), 0, UnityEngine.Random.Range(-tileOutlier, tileOutlier));
                                        vector3.x += tile.transform.position.x;
                                        vector3.z += tile.transform.position.z;

                                        if (oldVector3.Count == 0)
                                        {
                                            foundPos = true;
                                        }
                                        else
                                        {
                                            bool continueLoop = true;
                                            for (int n = 0; n < oldVector3.Count && continueLoop; n++)
                                            {

                                                if (Vector3.Distance(oldVector3[n], vector3) > oldSize[n] + navMeshSize + ranAmbentObjectSize)
                                                {
                                                    foundPos = true;
                                                }
                                                else
                                                {
                                                    foundPos = false;
                                                    continueLoop = false;
                                                }
                                            }
                                            reposisionTimes++;
                                        }
                                    }

                                    while (reposisionTimes <= 10 && !foundPos);
                                    if (foundPos)
                                    {


                                        GameObject ambientObject = Instantiate(ranAmbentObject, vector3, new Quaternion(0, 0, 0, 0));
                                        oldVector3.Add(vector3);
                                        oldSize.Add(ranAmbentObjectSize);
                                        ambientObject.transform.parent = tile.transform;

                                        ambientObject.transform.Rotate(0, UnityEngine.Random.Range(0, 360), 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public bool SetLoadResource
    {
        set => LoadResource = value;
    }


}

[System.Serializable]
public class FileHeader
{
    public int FileVersion = 1;
    public int GridXSize = 0;
    public int GridYSize = 0;
    public TileEntry[] Data;
}

