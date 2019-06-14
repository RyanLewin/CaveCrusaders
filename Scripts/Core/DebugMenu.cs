using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
public class DebugMenu : MonoBehaviour
{

    bool IsActive = false;
    bool HideFogOfWar = false;
    SilhouetteController controller;
    string XText = "";
    string YText = "";
    FluidEngine fe = null;
    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<SilhouetteController>();
        fe = transform.parent.GetComponentInChildren<FluidEngine>();
        if (fe != null)
        {
            XText = fe.TargetPos.x.ToString();
            YText = fe.TargetPos.y.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2) && Input.GetKey(KeyCode.LeftShift))
        {
            IsActive = !IsActive;
        }
    }

    private void OnGUI()
    {
        if (!IsActive)
        {
            return;
        }
        GUILayout.Window(442, new Rect(100, 100, 100, 100), DoMyWindow, "Debug Menu");
    }

    void DoMyWindow(int windowID)
    {


        if (fe != null)
        {
            GUILayout.BeginHorizontal();
            XText = GUILayout.TextField(XText);
            YText = GUILayout.TextField(YText);
            int XCoord, YCoord = 0;
            int.TryParse(XText, out XCoord);
            int.TryParse(YText, out YCoord);
            fe.TargetPos = new Vector2Int(XCoord, YCoord);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Place " + (fe.DebugType == Fluid.FluidType.Lava ? "Lava" : "Water")))
            {
                if (fe.DebugType == Fluid.FluidType.Lava)
                {
                    fe.DebugType = Fluid.FluidType.Water;
                }
                else
                {
                    fe.DebugType = Fluid.FluidType.Lava;
                }
            }
            fe.DEBUG_ADDFLuid = GUILayout.Toggle(fe.DEBUG_ADDFLuid, "Hold");
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button(HideFogOfWar ? "Show FOW" : "Hide FOW"))
        {
            HideFogOfWar = !HideFogOfWar;
            WorldController.SetFowState(!HideFogOfWar);
        }
        if (GUILayout.Button("Add Ore"))
        {
            WorldController._worldController._oreCount += 500;
        }
        if (GUILayout.Button("Add Crystals"))
        {
            WorldController._worldController._energyCrystalsCount += 100;
        }
        if (GUILayout.Button("Upgrade HQ"))
        {
            WorldController._worldController._HQ.UpgradeBuilding(false);
        }
        if (GUILayout.Button("Add Worker"))
        {
            WorldController._worldController._HQ.AddUnit(false);
        }
        if (controller != null)
        {
            if (GUILayout.Button("Switch Outline Mode: " + controller.GetMode().ToString()))
            {
                if (controller != null)
                {
                    int newmode = ((int)controller.GetMode() + 1) % (int)SilhouetteController.OutLineMode.Limit;
                    controller.SetMode((SilhouetteController.OutLineMode)newmode);
                }
            }
            controller.ShowDepth = GUILayout.Toggle(controller.ShowDepth, "Debug depth");
        }

        if (GUILayout.Button("Reset Fog"))
        {
            WorldController.SetFowState(!HideFogOfWar, true);
        }

        if (GUILayout.Button("Instant Mine: " + WorldController._worldController.DEBUG_INSTANT_MINE.ToString()))
        {
            WorldController._worldController.DEBUG_INSTANT_MINE = !WorldController._worldController.DEBUG_INSTANT_MINE;
        }
        if (GUILayout.Button("Fast Workers: " + WorldController._worldController.DEBUG_FAST_WORKER_MOVE.ToString()))
        {
            WorldController._worldController.DEBUG_FAST_WORKER_MOVE = !WorldController._worldController.DEBUG_FAST_WORKER_MOVE;
        }
        bool BreakerActive = WorldController._worldController.MouseSelectionControl.GetMode() == MouseSelectionController.CurrentSelectionMode.Breaker;
        if (GUILayout.Button((BreakerActive ? "Disable" : "Enable") + " Tile Breaker Mode"))
        {
            WorldController._worldController.MouseSelectionControl.SetMode(BreakerActive ? MouseSelectionController.CurrentSelectionMode.Select : MouseSelectionController.CurrentSelectionMode.Breaker);
        }
        if (GUILayout.Button("Enter Editor Mode"))
        {
            WorldController.GetWorldController.SetGamePlayMode(WorldController.CurrentGameMode.Editor);
        }
        if (GUILayout.Button("Enter Play Mode"))
        {
            WorldController.GetWorldController.SetGamePlayMode(WorldController.CurrentGameMode.Play);
        }
        if (GUILayout.Button("Fast Growth " + WorldController.GetWorldController.DEBUG_FAST_GROW.ToString()))
        {
            WorldController.GetWorldController.DEBUG_FAST_GROW = !WorldController.GetWorldController.DEBUG_FAST_GROW;
            var v = FindObjectsOfType<MushroomCluster>();
            foreach (MushroomCluster MC in v)
            {
                //force a reset
                MC.ResetSpawnTimer();
            }
        }
        WorldController.GetWorldController.DEBUGSPAWNGARY = false;
        if (GUILayout.Button("Spawn Gary"))
        {
            WorldController.GetWorldController.DEBUGSPAWNGARY = true;
            HQ H = WorldController.GetWorldController._HQ;
            if (H != null)
            {
                H.AddUnit(false);
            }
          
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
}
#endif