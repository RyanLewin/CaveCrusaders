using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLibrary : MonoBehaviour
{
    [SerializeField]
    GameObject[] _TilePrefabs;
    [SerializeField]
    Texture2D[] OrderTextures;
    [SerializeField]
    Tile.TileTypeID[] TileIdsForBuildingOrderTextures;
    [SerializeField]
    Texture2D[] BuildingOrderTextures;
    Dictionary<int, GameObject> TileIdMap = new Dictionary<int, GameObject>();
    Dictionary<Tile.TileTypeID, Texture2D> TileTextureMap = new Dictionary<Tile.TileTypeID, Texture2D>();
    public GameObject GetTilePrefab(int tileid)
    {
        GameObject value = null;
        if (TileIdMap.TryGetValue(tileid, out value))
        {
            return value;
        }
        Debug.LogError("Failed to Find Tile with ID " + tileid);
        return null;
    }
    public Dictionary<int, GameObject> GetTileList()//todo: ref?
    {
        if (TileIdMap.Count == 0)
        {
            Awake();
        }
        return TileIdMap;
    }
    void Awake()
    {
        if (TileIdMap.Count > 0)
        {
            return;
        }
        for (int i = 0; i < _TilePrefabs.Length; i++)
        {
            if (_TilePrefabs[i] == null)
            {
                Debug.LogWarning("_TilePrefabs[i] == null");
                continue;
            }
            Tile t = _TilePrefabs[i].GetComponent<Tile>();
            if (t == null)
            {
                continue;
            }
            int id = t.GetID();
            TileIdMap.Add(id, _TilePrefabs[i]);
        }
        for (int i = 0; i < BuildingOrderTextures.Length; i++)
        {
            TileTextureMap.Add(TileIdsForBuildingOrderTextures[i], BuildingOrderTextures[i]);
        }
    }
    public static TileLibrary Get()
    {
        return WorldController.GetWorldController.GetComponentInChildren<TileLibrary>();
    }
    public Texture2D GetOrderIcon(OrderVisualizer.OrderVisualType Type)
    {
        if (Type == OrderVisualizer.OrderVisualType.None)
        {
            return null;
        }
        int Targetindex = (int)Type;
        if (Targetindex >= OrderTextures.Length)
        {
            return null;
        }
        return OrderTextures[Targetindex];
    }
    public Texture2D GetBuildingTexture(Tile.TileTypeID type)
    {
        Texture2D value = null;
        if (TileTextureMap.TryGetValue(type, out value))
        {
            return value;
        }
        return OrderTextures[0];
    }
}
