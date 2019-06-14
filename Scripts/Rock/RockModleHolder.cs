using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockModleHolder : MonoBehaviour
{
    [SerializeField] GameObject _defultTile;
    [SerializeField] GameObject _dirtRockTile;
    [SerializeField] GameObject _dirtRockEnergy;
    [SerializeField] GameObject _looseRockTile;
    [SerializeField] GameObject _looseRockEnergy;
    [SerializeField] GameObject _HardRockTile;
    [SerializeField] GameObject _HardRockEnergy;
    [SerializeField] GameObject _rubbleTile;
    [SerializeField] GameObject _energyRubbleTile;
    [SerializeField] GameObject _rubble;
    [SerializeField] GameObject _cornerDouble;
    [SerializeField] GameObject _cornerQuad;
    [SerializeField] GameObject _cornerSingle;
    [SerializeField] GameObject _full;
    [SerializeField] GameObject _invertCorner;
    [SerializeField] GameObject _invertQuad;
    [SerializeField] GameObject _invertDouble;
    [SerializeField] GameObject _invertDoubleB;
    [SerializeField] GameObject _invertDoubleWall;
    [SerializeField] GameObject _invertSingle;
    [SerializeField] GameObject _invertTriple;
    [SerializeField] GameObject _invertWallA;
    [SerializeField] GameObject _invertWallB;
    [SerializeField] GameObject _wallDouble;
    [SerializeField] GameObject _wallSingle;
    [SerializeField] GameObject _cornerQuadSingleDag;
    [SerializeField] GameObject _cornerQuadDoubleDag;
    [SerializeField] GameObject _cornerQuadDoubleDagDag;
    [SerializeField] GameObject _cornerQuadTripleDag;
    [SerializeField] GameObject _cornerQuadQuadDag;
    [SerializeField] GameObject _cornerSingleDag;
    [SerializeField] GameObject _cornerDoubleSingleDagLeft;
    [SerializeField] GameObject _cornerDoubleSingleDagRight;
    [SerializeField] GameObject _cornerDoubleDoubleDag;
    [SerializeField] GameObject _invertCornerDag;
    [SerializeField] GameObject _mushroomCluster;
    [SerializeField] GameObject _landSlideDetector;
    [SerializeField] GameObject _lavaSounds;
    [SerializeField] List<GameObject> _ambientObjects = new List<GameObject>();

    public GameObject DefultTile { get { return _defultTile; } }
    public GameObject DirtRockTile { get { return _dirtRockTile; } }
    public GameObject DirtRockEnergy { get { return _dirtRockEnergy; } }
    public GameObject LooseRockTile { get { return _looseRockTile; } }
    public GameObject LooseRockEnergy { get { return _looseRockEnergy; } }
    public GameObject HardRockTile { get { return _HardRockTile; } }
    public GameObject HardRockEnergy { get { return _HardRockEnergy; } }
    public GameObject RubbleTile { get { return _rubbleTile; } }
    public GameObject EnergyRubbleTile { get { return _energyRubbleTile; } }
    public GameObject Rubble { get { return _rubble; } }
    public GameObject CornerDouble { get { return _cornerDouble; } }
    public GameObject CornerQuad { get { return _cornerQuad; } }
    public GameObject CornerSingle { get { return _cornerSingle; } }
    public GameObject Full { get { return _full; } }
    public GameObject InvertCorner { get { return _invertCorner; } }
    public GameObject InvertQuad { get { return _invertQuad; } }
    public GameObject InvertDouble { get { return _invertDouble; } }
    public GameObject InvertDoubleB { get { return _invertDoubleB; } }
    public GameObject InvertDoubleWall { get { return _invertDoubleWall; } }
    public GameObject InvertSingle { get { return _invertSingle; } }
    public GameObject InvertTriple { get { return _invertTriple; } }
    public GameObject InvertWallA { get { return _invertWallA; } }
    public GameObject InvertWallB { get { return _invertWallB; } }
    public GameObject WallDouble { get { return _wallDouble; } }
    public GameObject WallSingle { get { return _wallSingle; } }
    public GameObject CornerQuadSingleDag { get { return _cornerQuadSingleDag; } }
    public GameObject CornerQuadDoubleDag { get { return _cornerQuadDoubleDag; } }
    public GameObject CornerQuadDoubleDagDag { get { return _cornerQuadDoubleDagDag; } }
    public GameObject CornerQuadTripleDag { get { return _cornerQuadTripleDag; } }
    public GameObject CornerQuadQuadDag { get { return _cornerQuadQuadDag; } }
    public GameObject CornerSingleDag { get { return _cornerSingleDag; } }
    public GameObject CornerDoubleSingleDagLeft { get { return _cornerDoubleSingleDagLeft; } }
    public GameObject CornerDoubleSingleDagRight { get { return _cornerDoubleSingleDagRight; } }
    public GameObject CornerDoubleDoubleDag { get { return _cornerDoubleDoubleDag; } }
    public GameObject InvertCornerDag { get { return _invertCornerDag; } }
    public GameObject MushroomCluster { get { return _mushroomCluster; } }
    public GameObject LandSlideDetector { get { return _landSlideDetector; } }
    public GameObject LavaSound { get { return _lavaSounds; } }
    public List<GameObject> AmbientObjects { get { return _ambientObjects; } }
    private void Start()
    {
        StaticMapInfo.RockModleHolder = this;
    }



}
