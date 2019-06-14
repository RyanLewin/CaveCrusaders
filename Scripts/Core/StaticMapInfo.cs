using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class StaticMapInfo
{
    static string _level;
    static bool _IsLevelInBuilt;
    public static bool LoadingIntoLevelEditor;
    static TileMap3D _map;
    static RockModleHolder _rockModleHolder;
    static public WorldController.CurrentGameMode LevelEditorMode = WorldController.CurrentGameMode.Editor;
    static public TileMap3D Map { set { _map = value; } get { return _map; } }
    static public RockModleHolder RockModleHolder { set { _rockModleHolder = value; } get { return _rockModleHolder; } }
    static public string Level { get { return _level; } }
    static public bool IsLevelInBuilt { get { return _IsLevelInBuilt; } }
    public static string LevelEditorLevel;
    public static void SetLevelLoadData(string name, bool IsBuilt)
    {
        _IsLevelInBuilt = IsBuilt;
        _level = name;
    }
    public static void LoadIntoEditor(bool LiveEdit)
    {
        LoadingIntoLevelEditor = true;
        LevelEditorMode = LiveEdit ? WorldController.CurrentGameMode.EditorPlay : WorldController.CurrentGameMode.Editor;
    }

    /// <summary>
    /// Checks if there is a wall in the X and Y location
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    static public bool isWall(int x, int y)
    {
        if (x >= 0 && y >= 0 && x <= StaticMapInfo.Map.GridXSize_ && y <= StaticMapInfo.Map.GridYSize_)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y);
            if (tile == null)
            {
                return false;
            }
            if (tile.tag == "RockTile")
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Checks all tiles, and ajusts walls
    /// </summary>
    static public void RockProximty()
    {
        for (int x = 0; x < Map.GridXSize_; x++)
        {
            for (int y = 0; y < Map.GridYSize_; y++)
            {
                Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y);
                if (tile.tag == "RockTile")
                {
                    RockScript tileRockSript = tile.GetComponent<RockScript>();
                    tileRockSript.RockNorth = StaticMapInfo.isWall(x, y + 1);
                    tileRockSript.RockNorthEast = StaticMapInfo.isWall(x + 1, y + 1);
                    tileRockSript.RockEast = StaticMapInfo.isWall(x + 1, y);
                    tileRockSript.RockSouthEast = StaticMapInfo.isWall(x + 1, y - 1);
                    tileRockSript.RockSouth = StaticMapInfo.isWall(x, y - 1);
                    tileRockSript.RockSouthWest = StaticMapInfo.isWall(x - 1, y - 1);
                    tileRockSript.RockWest = StaticMapInfo.isWall(x - 1, y);
                    tileRockSript.RockNorthWest = StaticMapInfo.isWall(x - 1, y + 1);
                }
            }
        }
    }


}

