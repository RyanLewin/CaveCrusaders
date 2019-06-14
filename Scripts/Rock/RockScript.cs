using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
public class RockScript : Tile
{

    [Header("Rock Setup")]
    [SerializeField] List<AudioClip> _landSlidAudio = new List<AudioClip>();
    [SerializeField] GameObject _cumbleSource;
    [SerializeField] GameObject _rockGameObject;
    [SerializeField] float _brakeTime;
    float _totalBrakeTime;
    [SerializeField] bool _energyCrystal;
    [SerializeField] GameObject _renforecedWall;

    [SerializeField] int _swingBrakeAmount;
    int _totalSwingBrakeAmount;
    [SerializeField] Type _type;
    [SerializeField] Material _startMaterial;
    [SerializeField] Material _crackmaterial;
    [SerializeField] Texture _particalTexutre;
    [SerializeField] VisualEffect _rockBrakeEffect;
    [SerializeField] VisualEffect _landSlideEffect;
    [Header("Debug")]
    [SerializeField] bool _beingMined; //Serialized for Debuging
    [SerializeField] bool renforece;

    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _landslideClip;

    WorldController _worldController;
    RockModleHolder _rockModleHolder;
    GameObject _landSlideRock;
    GameObject _landSlideRubble;
    Material _baseMaterial;


    float _stateSwichTime;
    float _stateSwichWait;

    bool _rockNorth;
    bool _rockNorthEast;
    bool _rockEast;
    bool _rockSouthEast;
    bool _rockSouth;
    bool _rockSouthWest;
    bool _rockWest;
    bool _rockNorthWest;

    bool _reinforeced = false;
    bool _landSlideInEffect = false;


    public bool RockNorth { set { _rockNorth = value; } }
    public bool RockNorthEast { set { _rockNorthEast = value; } }
    public bool RockEast { set { _rockEast = value; } }
    public bool RockSouthEast { set { _rockSouthEast = value; } }
    public bool RockSouth { set { _rockSouth = value; } }
    public bool RockSouthWest { set { _rockSouthWest = value; } }
    public bool RockWest { set { _rockWest = value; } }
    public bool RockNorthWest { set { _rockNorthWest = value; } }
    public bool IsReinforeced { get { return _reinforeced; } }
    public bool LandSlideInEffect { get { return _landSlideInEffect; } }
    public enum Type { Dirt, LooseRock, HardRock, SolidRock }

    void Awake()
    {

        _totalSwingBrakeAmount = _swingBrakeAmount;
        _totalBrakeTime = _brakeTime;
        _baseMaterial = new Material(_startMaterial);
        Material material = new Material(_startMaterial);
        _startMaterial = material;
        Material newCrack = new Material(_crackmaterial);
        _crackmaterial = newCrack;
        _stateSwichWait = 0.01f / _brakeTime;
        _stateSwichTime = _brakeTime - _stateSwichWait;
        _beingMined = false;
        if (_type == Type.SolidRock)
        {
            _IsSolid = true;
        }
        else
        {
            _IsDestructable = true;

        }

        foreach (MeshRenderer mesh in _rockGameObject.GetComponentsInChildren<MeshRenderer>())
        {
            Material[] mats = mesh.materials;
            Material[] newMat = new Material[mats.Length + 1];

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = _startMaterial;
                newMat[i] = mats[i];
            }
            newMat[mats.Length] = _crackmaterial;


            mesh.materials = newMat;
        }

    }
    void Start()
    {
        _worldController = WorldController.GetWorldController;
        _rockModleHolder = StaticMapInfo.RockModleHolder;
        if (_energyCrystal)
        {
            switch (_type)
            {
                case Type.Dirt:
                    _worldController._energyCrystalsOnMap += 1;
                    break;
                case Type.LooseRock:
                    _worldController._energyCrystalsOnMap += 2;
                    break;
                case Type.HardRock:
                    _worldController._energyCrystalsOnMap += 3;
                    break;
            }
        }

        changeModle();
    }

    public override int GetID()
    {
        if (_type == Type.Dirt)
        {
            if (_energyCrystal)
            {
                return (int)TileTypeID.DirtEnergy;
            }
            return (int)TileTypeID.Dirt;
        }
        if (_type == Type.LooseRock)
        {
            if (_energyCrystal)
            {
                return (int)TileTypeID.LooseRockEnergy;
            }
            return (int)TileTypeID.LooseRock;
        }
        if (_type == Type.HardRock)
        {
            if (_energyCrystal)
            {
                return (int)TileTypeID.HardRockEnergy;
            }
            return (int)TileTypeID.HardRock;
        }
        return (int)TileTypeID.SolidRock;

    }

    protected override void OnTileStart()
    {
        switch (_type)
        {
            case Type.Dirt:
                _TileDurablity = 10;
                break;
            case Type.LooseRock:
                _TileDurablity = 20;
                _IsDestructable = false;
                _IsSolid = true;
                break;
            case Type.HardRock:
                _TileDurablity = 60;
                _IsDestructable = false;
                _IsSolid = true;
                break;
        }
        base.OnTileStart();
    }

    // Update is called once per frame
    protected override void TileUpdate()
    {
        if (_type != RockScript.Type.SolidRock)
        {
            if (_audioSource != null)
            {
                if (!_landSlideInEffect && _audioSource.isPlaying)
                {
                    _audioSource.volume -= Time.deltaTime * 10;
                    if (_audioSource.volume <= 0)
                    {
                        _audioSource.Stop();

                    }
                }

            }

            if (_landSlideRock != null)
            {
                _landSlideRock.transform.Translate(new Vector3(0, Time.deltaTime * 2, 0));
            }
            if (_landSlideRubble != null)
            {
                _landSlideRubble.transform.Translate(new Vector3(0, Time.deltaTime / 2, 0));
            }


            if (!_reinforeced && _renforecedWall.activeSelf)
            {
                _reinforeced = _renforecedWall.GetComponent<Reinforecments>();
            }
            if (_beingMined)
            {
                Mined(1);
            }

            ////debug
            if (renforece)
            {
                RenforceWall();
            }
        }
    }
    /// <summary>
    /// "Counts down the timer to brake the rock, will brake if 0"
    /// </summary>
    public void Mined(float speedModifer)
    {
        if (_type != Type.SolidRock)
        {
            if (_worldController.DEBUG_INSTANT_MINE)
            {
                _brakeTime = 0;
            }
            _brakeTime -= speedModifer * Time.deltaTime;
            if (_brakeTime <= 0)
            {
                DistroyRock();

            }

            if (_worldController._miningLevel == WorldController.MiningLevel.three)
            {
                Color crackColour = _startMaterial.GetColor("_EmissiveColor");

                float colourChange = ((speedModifer * Time.deltaTime) / (10 / _totalBrakeTime));// * 255;

                crackColour.r += colourChange;
                crackColour.g += colourChange;
                crackColour.b += colourChange;

                _startMaterial.SetColor("_EmissiveColor", crackColour);
            }
            else
            { 
            Color crackColour = _crackmaterial.GetColor("_BaseColor");


            crackColour.a += (speedModifer * Time.deltaTime) / (2 / _totalBrakeTime);

            _crackmaterial.SetColor("_BaseColor", crackColour);
        }
        }
    }

    public void PickaxeMined()
    {
        if (_type != Type.SolidRock)
        {
            if (_worldController.DEBUG_INSTANT_MINE)
            {
                _swingBrakeAmount = 0;
            }
            _swingBrakeAmount--;
            if (_swingBrakeAmount <= 0)
            {
                DistroyRock();

            }
            else
            {
                Color crackColour = _crackmaterial.GetColor("_BaseColor");

                crackColour.a += 2 / (float)_totalSwingBrakeAmount;

                _crackmaterial.SetColor("_BaseColor", crackColour);


            }
            _brakeTime -= _brakeTime / (_swingBrakeAmount + 1);
            _stateSwichTime -= _stateSwichWait;
        }
    }

    public void DistroyRock()
    {
        if (_rockBrakeEffect != null)
        {
            _rockBrakeEffect.enabled = true;
            _rockBrakeEffect.SetTexture("RockTexture", _particalTexutre);
            _rockBrakeEffect.transform.parent = null;
            _rockBrakeEffect.GetComponent<DistroyPartical>().enabled = true;

        }

        if (_landSlideRock != null)
        {
            Object.Destroy(_landSlideRock);
            Object.Destroy(_landSlideRubble);
        }
        Vector3 location = transform.position;
        Instantiate(_cumbleSource,new Vector3(location.x,location.y,location.z),new Quaternion(0,0,0,0));

            if (_worldController._landSlides)
        {
            int rand = Random.Range(0, 11);
            if (rand == 0)
            {
               LandSlide(true);
            }
        }
       // LandSlide(true);
        _worldController._landslideRocks.Remove(this);
        ChangleLocalWalls(false, x, y);
        if (_energyCrystal)
        {
            _rockModleHolder.EnergyRubbleTile.GetComponent<RubbleScript>().ChangeTexture(_type);
            StaticMapInfo.Map.UpdateTile(x, y, _rockModleHolder.EnergyRubbleTile);
        }
        else
        {
            _rockModleHolder.RubbleTile.GetComponent<RubbleScript>().ChangeTexture(_type);
            StaticMapInfo.Map.UpdateTile(x, y, _rockModleHolder.RubbleTile);
        }
    }


    public void RenforceWall()
    {
        _renforecedWall.SetActive(true);
        _renforecedWall.GetComponent<Reinforecments>().SetWalls(!_rockNorth, !_rockEast, !_rockSouth, !_rockEast);
        renforece = false;
    }

   public void LandSlide(bool rock)
    {
        if (!(_rockNorth || _rockNorthEast || _rockEast || _rockSouthEast || _rockSouth || _rockSouthWest || _rockWest || _rockNorthWest))
        {
            return;
        }

        int side = Random.Range(0, 8);
        //Y = north // X =East
        int loop = 0;
        do
        {
            loop++;

            switch (side)
            {
                case 0:
                    if (_rockNorth)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y + 1);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                    //adjestentRock.RockSouth = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }
                        }
                    }
                  
      
                    
                    break;
                case 1:
                    if (_rockNorthEast)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x + 1, y + 1);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                   // adjestentRock.RockSouthWest = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }

                        }
                    }
                    break;
                case 2:
                    if (_rockEast)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x + 1, y);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                   // adjestentRock.RockWest = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }

                                return;
                            }
                        }
                    }
                   
                 
                    break;
                case 3:
                    if (_rockSouthEast)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x + 1, y - 1);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                  //  adjestentRock.RockNorthWest = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }
                        }
                    }

               
                
                    break;
                case 4:
                    if (_rockSouth)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x, y - 1);
                        if (tile != null && tile.tag == "RockTile")
                        {

                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                               //     adjestentRock.RockNorth = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }

                        }
                    }
                   
                    break;
                case 5:
                    if (_rockSouthWest)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x - 1, y - 1);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                    //adjestentRock.RockNorthEast = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }

                        }
                    }
                  
                    break;
                case 6:
                    if (_rockWest)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x - 1, y);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                  //  adjestentRock.RockEast = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }

                        }
                    }
                
                    break;
                case 7:
                    if (_rockNorthWest)
                    {
                        Tile tile = StaticMapInfo.Map.GetTileAtPos(x - 1, y + 1);
                        if (tile != null && tile.tag == "RockTile")
                        {
                            RockScript adjestentRock = tile.GetComponent<RockScript>();
                            if (adjestentRock.RockType != Type.SolidRock && !adjestentRock.LandSlideInEffect)
                            {
                                if (!adjestentRock.IsReinforeced)
                                {
                                   // adjestentRock.RockSouthEast = false;
                                    adjestentRock.ALandSlideHasOccurred(rock);
                                }
                                return;
                            }

                        }
                    }
                    side = -1;
                    break;
            }
            side++;
        }
        while (loop < 9);
    }

    public void ALandSlideHasOccurred(bool rock)
    {
        if ((_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest))
        {
            return;
        }

        _landSlideEffect.enabled = true;
        _landSlideInEffect = true;
        if (_landSlidAudio.Count != 0)
        {
            _worldController._soundManager.PlayAnnouncement(_landSlidAudio[Random.Range(0, _landSlidAudio.Count)]);
            
        }
        _audioSource.volume = 1;
        _audioSource.clip = _landslideClip;
        _audioSource.Play();
        //Y = north // X =East


        int side = Random.Range(0, 8);
        int loop = 0;
        do
        {
            loop++;
            switch (side)
            {
                case 0:

                    if (!_rockNorth)
                    {
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x, y + 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x, y + 1));
                        }
                        
                        return;
                    }
                    side++;

                    break;
                case 1:

                    if (!_rockNorthEast)
                    {
                        _landSlideEffect.transform.Rotate(0, 45, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x + 1, y + 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x + 1, y + 1));
                        }
                        return;

                    }
                    side++;

                    break;
                case 2:

                    if (!_rockEast)
                    {
                        _landSlideEffect.transform.Rotate(0, 90, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x + 1, y));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x + 1, y));
                        }
                        return;
                    }
                    side++;
                    break;
                case 3:

                    if (!_rockSouthEast)
                    {
                        _landSlideEffect.transform.Rotate(0, 135, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x + 1, y - 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x + 1, y - 1));
                        }
                        return;
                    }
                    side++;
                    break;
                case 4:

                    if (!_rockSouth)
                    {
                        _landSlideEffect.transform.Rotate(0, 180, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x, y - 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x, y - 1));
                        }
                        return;
                    }
                    side++;
                    break;
                case 5:

                    if (!_rockSouthWest)
                    {
                        _landSlideEffect.transform.Rotate(0, 225, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x - 1, y - 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x + 1, y - 1));
                        }
                        return;
                    }
                    side++;
                    break;
                case 6:

                    if (!_rockWest)
                    {
                        _landSlideEffect.transform.Rotate(0, 270, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x - 1, y));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x - 1, y));
                        }
                            return;
                    }
                    side++;
                    break;
                case 7:

                    if (!_rockNorthWest)
                    {
                        _landSlideEffect.transform.Rotate(0, 315, 0);
                        if (rock)
                        {
                            StartCoroutine(MakeNewRock(x - 1, y + 1));
                        }
                        else
                        {
                            StartCoroutine(MakeNewRubble(x - 1, y + 1));
                        }
                        return;
                    }
                    side = 0;
                    break;
            }
        }
        while (loop < 9);
    }

    IEnumerator MakeNewRock(int rockX, int rockY)
    {
        Vector3 tileLocation = StaticMapInfo.Map.GetTileAtPos(rockX, rockY).transform.position;
        if (_landSlideRock != null)
        {
            Object.Destroy(_landSlideRock);
            _landSlideRock = null;
        }

        Debug.Log("A Land Slide Has Occurred");
        _landSlideRock = Instantiate(_rockModleHolder.CornerQuad, new Vector3(tileLocation.x,tileLocation.y - 8,tileLocation.z), new Quaternion(0, 0, 0, 0));
        GameObject landSlideDectecor = Instantiate(_rockModleHolder.LandSlideDetector, new Vector3(tileLocation.x, tileLocation.y - 8, tileLocation.z), new Quaternion(0, 0, 0, 0));
        landSlideDectecor.transform.parent = _landSlideRock.transform;
        _landSlideRock.GetComponentInChildren<MeshRenderer>().material = _baseMaterial;
        yield return new WaitForSeconds(4f);
        _landSlideEffect.Stop();//.enabled = false;
        Object.Destroy(_landSlideRock);
        _landSlideRock = null;
        RockModleHolder rockModleHolder = _rockModleHolder;
        GameObject rock = rockModleHolder.DirtRockTile;

        Type type = _type;
        bool energyCrystal = _energyCrystal;
       Tile lastTile = StaticMapInfo.Map.GetTileAtPos(rockX, rockY);
        if (lastTile.tag == "Rubble")
        {
            RubbleScript rubble = StaticMapInfo.Map.GetTileAtPos(rockX, rockY).GetComponent<RubbleScript>();
            type = rubble.RockType;
            energyCrystal = rubble.EnergyCrystal;

        }
        else if(lastTile.tag == "Floor")
        {
            _worldController._defultTile.Remove(lastTile);
        }


        if (type == Type.Dirt)
        {
            if (energyCrystal)
            {
                rock = rockModleHolder.DirtRockEnergy;
            }
            else
            {
                rock = rockModleHolder.DirtRockTile;
            }
        }
        if (type == Type.LooseRock)
        {
            if ((int)_worldController._miningLevel == 0)
            {
                if (energyCrystal)
                {
                    rock = rockModleHolder.DirtRockEnergy;
                }
                else
                {
                    rock = rockModleHolder.DirtRockTile;
                }
            }
            else
            {
                if (energyCrystal)
                {
                    rock = rockModleHolder.LooseRockEnergy;
                }
                else
                {
                    rock = rockModleHolder.LooseRockTile;
                }
            }

        }
        if (type == Type.HardRock || type == Type.SolidRock)
        {
            if ((int)_worldController._miningLevel == 0)
            {
                if (energyCrystal)
                {
                    rock = rockModleHolder.DirtRockEnergy;
                }
                else
                {
                    rock = rockModleHolder.DirtRockTile;
                }
            }
            else if ((int)_worldController._miningLevel == 1)
            {
                if (energyCrystal)
                {
                    rock = rockModleHolder.LooseRockEnergy;
                }
                else
                {
                    rock = rockModleHolder.LooseRockTile;
                }
            }
            else
            {
                if (energyCrystal)
                {
                    rock = rockModleHolder.HardRockEnergy;
                }
                else
                {
                    rock = rockModleHolder.HardRockTile;
                }
            }
        }

        TileMap3D map = StaticMapInfo.Map;


        List<Worker> deadWorkers = new List<Worker>();

        foreach (Unit unit in _worldController._workers)
        {
    
            if (unit.GetComponent<Worker>() != null)
            {
                Tile unitTile = map.FindCell(unit.transform.position);
                if (rockX == unitTile.X && rockY == unitTile.Y)
                {
                    deadWorkers.Add(unit.GetComponent<Worker>());
                }
            }
        }

        foreach (Worker deadWorker in deadWorkers)
        {
            deadWorker.DeathByLandslide();
        }

        StaticMapInfo.Map.UpdateTile(rockX, rockY, rock);
        RockScript tileRockScript = StaticMapInfo.Map.GetTileAtPos(rockX, rockY).GetComponent<RockScript>();
        _worldController._landslideRocks.Add(tileRockScript);
        tileRockScript.RockNorth = StaticMapInfo.isWall(rockX, rockY + 1);
        tileRockScript.RockNorthEast = StaticMapInfo.isWall(rockX + 1, rockY + 1);
        tileRockScript.RockEast = StaticMapInfo.isWall(rockX + 1, rockY);
        tileRockScript.RockSouthEast = StaticMapInfo.isWall(rockX + 1, rockY - 1);
        tileRockScript.RockSouth = StaticMapInfo.isWall(rockX, rockY - 1);
        tileRockScript.RockSouthWest = StaticMapInfo.isWall(rockX - 1, rockY - 1);
        tileRockScript.RockWest = StaticMapInfo.isWall(rockX - 1, rockY);
        tileRockScript.RockNorthWest = StaticMapInfo.isWall(rockX - 1, rockY + 1);
        tileRockScript.ChangleLocalWalls(true, rockX, rockY);
        tileRockScript.changeModle();
        StartCoroutine(DisableLandSlide());
    }


    IEnumerator MakeNewRubble(int rubbleX, int rubbleY)
    {
        Vector3 tileLocation = StaticMapInfo.Map.GetTileAtPos(rubbleX, rubbleY).transform.position;
        if (_landSlideRubble != null)
        {
            Object.Destroy(_landSlideRubble);
            _landSlideRubble = null;
        }

        Debug.Log("A Land Slide Has Occurred");
        _landSlideRubble = Instantiate(_rockModleHolder.Rubble, new Vector3(tileLocation.x, tileLocation.y - 1.7f, tileLocation.z), new Quaternion(0, 0, 0, 0));

        _landSlideRubble.GetComponentInChildren<MeshRenderer>().material = _baseMaterial;
        yield return new WaitForSeconds(4f);
        _landSlideEffect.Stop();//.enabled = false;
        Object.Destroy(_landSlideRubble);
        _landSlideRubble = null;
        RockModleHolder rockModleHolder = _rockModleHolder;
        GameObject rubble = rockModleHolder.RubbleTile;
   
        Type type = _type;
        bool energyCrystal = _energyCrystal;
        Tile lastTile = StaticMapInfo.Map.GetTileAtPos(rubbleX, rubbleY);
        if (lastTile.tag == "Rubble")
        {
            RubbleScript rubbleScript = StaticMapInfo.Map.GetTileAtPos(rubbleX, rubbleY).GetComponent<RubbleScript>();
            type = rubbleScript.RockType;
            energyCrystal = rubbleScript.EnergyCrystal;

        }
        else if (lastTile.tag == "Floor")
        {
            _worldController._defultTile.Remove(lastTile);
        }

        if (energyCrystal)
        {
            rubble = rockModleHolder.EnergyRubbleTile;
        }

        rubble.GetComponent<RubbleScript>().ChangeTexture(type);


        TileMap3D map = StaticMapInfo.Map;


        List<Worker> deadWorkers = new List<Worker>();

        foreach (Unit unit in _worldController._workers)
        {
            if (unit.GetComponent<Worker>() != null)
            {
                Tile unitTile = map.FindCell(unit.transform.position);
                if (rubbleX == unitTile.X && rubbleY == unitTile.Y)
                {
                    deadWorkers.Add(unit.GetComponent<Worker>());
                }
            }
        }

        foreach (Worker deadWorker in deadWorkers)
        {
            deadWorker.Health -= 25;
        }

        StaticMapInfo.Map.UpdateTile(rubbleX, rubbleY, rubble);
        
        StartCoroutine(DisableLandSlide());
    }


    IEnumerator DisableLandSlide()
    {
        yield return new WaitForSeconds(3f);
        _landSlideEffect.transform.rotation = new Quaternion(0, 0, 0, 0);
        _landSlideEffect.enabled = false;
        _landSlideInEffect = false;

    }



    public bool BeingMined
    {
        set
        {
            _beingMined = value;
        }
    }

    /// <summary>
    /// "Adjusts the walls around the distored wall"
    /// </summary>
    public void ChangleLocalWalls(bool change, int tileX, int tileY)
    {
        //Y = north // X =East
        int mapXSize = StaticMapInfo.Map.GridXSize_;
        int mapYSize = StaticMapInfo.Map.GridYSize_;

        if (y + 1 <= mapYSize)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX, tileY + 1);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockSouth = change;
                adjestentRock.changeModle();
            }
        }

        if (y + 1 <= mapYSize && x + 1 <= mapXSize)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX + 1, tileY + 1);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockSouthWest = change;
                adjestentRock.changeModle();
            }
        }

        if (x + 1 <= mapXSize)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX + 1, tileY);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockWest = change;
                adjestentRock.changeModle();
            }
        }

        if (x + 1 <= mapXSize && y - 1 >= 0)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX + 1, tileY - 1);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockNorthWest = change;
                adjestentRock.changeModle();
            }
        }
        if (y - 1 >= 0)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX, tileY - 1);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockNorth = change;
                adjestentRock.changeModle();
            }
        }
        if (y - 1 >= 0 && x - 1 >= 0)
        {
            {
                Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX - 1, tileY - 1);
                if (tile != null && tile.tag == "RockTile")
                {
                    RockScript adjestentRock = tile.GetComponent<RockScript>();
                    adjestentRock.RockNorthEast = change;
                    adjestentRock.changeModle();
                }
            }
        }
        if (x - 1 >= 0)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX - 1, tileY);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockEast = change;
                adjestentRock.changeModle();
            }
        }
        if (x - 1 >= 0 && y + 1 <= mapYSize)
        {
            Tile tile = StaticMapInfo.Map.GetTileAtPos(tileX - 1, tileY + 1);
            if (tile != null && tile.tag == "RockTile")
            {
                RockScript adjestentRock = tile.GetComponent<RockScript>();
                adjestentRock.RockSouthEast = change;
                adjestentRock.changeModle();
            }
        }

    }



    public Type RockType => _type;

    /// <summary>
    /// "Changes the model relivent to the walls around it"
    /// </summary>
    public void changeModle()
    {
        if(_rockModleHolder == null)
        {
             _rockModleHolder=StaticMapInfo.RockModleHolder;
        }

        if (_renforecedWall.activeSelf)
        {
            _renforecedWall.GetComponent<Reinforecments>().SetWalls(!_rockNorth, !_rockEast, !_rockSouth, !_rockEast);
        }


        Object.Destroy(_rockGameObject);

        // Walls all around
        if (_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.Full, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }

        /*
         * One Side without wall
         */
        //north wall
        else if (!_rockNorth && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //east wall
        else if (_rockNorth && !_rockEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }
        //south wall
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouth && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //west wall
        else if (_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        /*
         * Corners
         */
        // walls on north and east
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouth && !_rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // walls on east and south
        else if (!_rockNorth && _rockEast && _rockSouthEast && _rockSouth && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // walls on south and west
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouth && _rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //walls on north and west
        else if (_rockNorth && !_rockEast && !_rockSouthEast && !_rockSouth && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        /*
         * Corners Dagonal
        */
        // walls on north and east
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouth && _rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // walls on east and south
        else if (!_rockNorth && _rockEast && _rockSouthEast && _rockSouth && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // walls on south and west
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouth && _rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //walls on north and west
        else if (_rockNorth && !_rockEast && _rockSouthEast && !_rockSouth && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }

        /*
         * One cornner with no wall
         */
        // No Noth East
        else if (_rockNorth && !_rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        // No South East
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // No South West 
        else if (_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // No North West 
        else if (_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertSingle, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        /*
         * no Two non diagnal cornners
         */
        // North Coreners
        else if (_rockNorth && !_rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        // East Corners
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        // South Corners
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // West Corners
        else if (_rockNorth && _rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        /*
       * no Two diagnal cornners
       */
        //North East and South West
        else if (_rockNorth && !_rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //North West and South East
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        /*
         *Three no cornners 
         */
        //North East rock
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertTriple, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // South East Rock 
        else if (_rockNorth && !_rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertTriple, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        // South West
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && _rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertTriple, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        // North West
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertTriple, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        /*
         *No Corners 
         */
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertQuad, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }

        /*
         * next to single Rock
         */
        //RockNorth
        else if (_rockNorth && !_rockEast && !_rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //RockEast
        else if (!_rockNorth && _rockEast && !_rockSouth && !_rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //RockSouth
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouth && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //RockWest
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }


        /*
    * next to single Rock, single Diagonal left
    */
        //RockNorth
        else if (_rockNorth && !_rockEast && _rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagLeft, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //RockEast
        else if (!_rockNorth && _rockEast && !_rockSouth && _rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagLeft, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //RockSouth
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouth && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagLeft, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //RockWest
        else if (!_rockNorth && _rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagLeft, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }


        /*
    * next to single Rock, single Diagonal Right
    */
        //RockNorth
        else if (_rockNorth && !_rockEast && !_rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagRight, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //RockEast
        else if (!_rockNorth && _rockEast && !_rockSouth && !_rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagRight, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //RockSouth
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouth && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagRight, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //RockWest
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleSingleDagRight, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }


        /*
         * next to single Rock, Double Diagonal Right
       */
        //RockNorth
        else if (_rockNorth && !_rockEast && _rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //RockEast
        else if (!_rockNorth && _rockEast && !_rockSouth && _rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //RockSouth
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouth && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //RockWest
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerDoubleDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }



        /*
       * One Side without wall + one no corner , wall A
       */
        //north wall
        else if (!_rockNorth && _rockEast && _rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallA, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //east wall
        else if (_rockNorth && !_rockEast && _rockSouth && _rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallA, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }
        //south wall
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouth && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallA, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //west wall
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallA, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        /*
        * One Side without wall + one no corner , wall B
        */
        //north wall
        else if (!_rockNorth && _rockEast && !_rockSouthEast && _rockSouth && _rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //east wall
        else if (_rockNorth && !_rockEast && _rockSouth && !_rockSouthWest && _rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }
        //south wall
        else if (_rockNorth && _rockNorthEast && _rockEast && !_rockSouth && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //west wall
        else if (_rockNorth && !_rockNorthEast && _rockEast && _rockSouthEast && _rockSouth && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertWallB, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        /*
         * One Side without wall + Two no corner
         */
        //north wall
        else if (!_rockNorth && _rockEast && !_rockSouthEast && _rockSouth && !_rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleWall, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //east wall
        else if (_rockNorth && !_rockEast && _rockSouth && !_rockSouthWest && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleWall, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }
        //south wall
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouth && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleWall, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //west wall
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouthEast && _rockSouth && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertDoubleWall, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        /*
         *  Coners, no wall oposite to corner
         */
        // walls on north and east
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouth && !_rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCorner, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // walls on east and south
        else if (!_rockNorth && _rockEast && !_rockSouthEast && _rockSouth && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCorner, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // walls on south and west
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouth && !_rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCorner, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //walls on north and west
        else if (_rockNorth && !_rockEast && !_rockSouthEast && !_rockSouth && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCorner, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }

        /*
    *  Coners, no wall oposite to corner with Diagonal
    */
        // walls on north and east
        else if (_rockNorth && !_rockNorthEast && _rockEast && !_rockSouth && _rockSouthWest && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCornerDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        // walls on east and south
        else if (!_rockNorth && _rockEast && !_rockSouthEast && _rockSouth && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCornerDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        // walls on south and west
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouth && !_rockSouthWest && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCornerDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //walls on north and west
        else if (_rockNorth && !_rockEast && _rockSouthEast && !_rockSouth && _rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.InvertCornerDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }


        /*
         * Wall Two sides stright
         */
        //north south
        else if (_rockNorth && !_rockEast && _rockSouth && !_rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //East West
        else if (!_rockNorth && _rockEast && !_rockSouth && _rockWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.WallDouble, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        /*
         * Single wall with one Dagiangle
         */
        //North West
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //South West
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //South East
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //North East
        else if (!_rockNorth && _rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadSingleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }

        /*
    * Single wall with Two Dagiangle
    */
        //North
        else if (!_rockNorth && _rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        //West
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //South
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //East
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        /*
        *Single wall with Two Dagiangle dinagonaly
        */
        //North East & South West
        else if (!_rockNorth && _rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDagDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);
        }
        //North West & South East
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadDoubleDagDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }

        /*
* Single wall with Triple Dagiangle
*/
        //Not NorthEast
        else if (!_rockNorth && !_rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadTripleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 180, 0);
        }
        //Not NorthWest
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && !_rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadTripleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 90, 0);

        }
        //Not SouthEast
        else if (!_rockNorth && _rockNorthEast && !_rockEast && !_rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadTripleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
            _rockGameObject.transform.Rotate(0, 270, 0);
        }
        //Not SouthWest
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && !_rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadTripleDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));

        }
        /*
     * Single wall four Dagiangle
     */
        else if (!_rockNorth && _rockNorthEast && !_rockEast && _rockSouthEast && !_rockSouth && _rockSouthWest && !_rockWest && _rockNorthWest)
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuadQuadDag, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }



        else
        {
            _rockGameObject = (GameObject)Instantiate(_rockModleHolder.CornerQuad, new Vector3(transform.position.x, transform.position.y, transform.position.z), new Quaternion(0, 0, 0, 0));
        }
        _rockGameObject.transform.parent = transform;
        //_rockGameObject.GetComponentInChildren<MeshRenderer>().material = startMaterial;



        foreach (MeshRenderer mesh in _rockGameObject.GetComponentsInChildren<MeshRenderer>())
        {
            Material[] mats = mesh.materials;
            Material[] newMat = new Material[mats.Length + 1];

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = _startMaterial;
                newMat[i] = mats[i];
            }
            newMat[mats.Length] = _crackmaterial;


            mesh.materials = newMat;
        }

    }

    protected override void OnTileDestroy()
    {
        _brakeTime = 0;
        ChangleLocalWalls(false, x, y);
    }

}
