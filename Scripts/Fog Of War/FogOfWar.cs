using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FogOfWar : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] LayerMask _fogLayer;
    [SerializeField] float _radius;
    GameObject _fogOfWar;
    [SerializeField] LayerMask _ignoreRayCast;

    Mesh _mesh;
    Vector3[] _vertices;
    Color[] _colours;

    List<int> _changedVectors = new List<int>();
    List<int> _monsterVisable = new List<int>();
    List<int> _changedVectorBuildings = new List<int>();
    List<Building> _BuldingFow = new List<Building>();
   

    float RadiusSqr { get { return _radius * _radius; } }

    int count = 0;

       int _yRowJump;
        int _spaceGap;
        int _yDiffrent;
        int _xDiffrent;

    

    void Start()
    {

        GameObject _fogOfWar = WorldController._worldController._fogOfWar;
        _mesh = _fogOfWar.GetComponent<MeshFilter>().mesh;
        _vertices = _mesh.vertices;
        _colours = new Color[_vertices.Length];
        for (int i = 0; i < _colours.Length; i++)
        {
            _colours[i] = Color.black;
        }
        UpdateColour();

         float vectorDistance = Mathf.RoundToInt((Vector3.Distance(_vertices[_mesh.triangles[0]], _vertices[_mesh.triangles[1]])) * _fogOfWar.transform.localScale.x * _fogOfWar.transform.localScale.y);
        _yRowJump = Mathf.RoundToInt(Mathf.Sqrt(_vertices.Length));
        _spaceGap = (int)((_radius) / vectorDistance);
        _yDiffrent = _yRowJump * _spaceGap;
        _xDiffrent = _spaceGap;
    }

    // Update is called once per frame
    void Update()
    {
        _changedVectors.Clear();
        _changedVectors = _changedVectorBuildings.Distinct().ToList();
        _monsterVisable.Clear();

        _fogOfWar = WorldController._worldController._fogOfWar;
        WorldController world = WorldController._worldController;
        _fogOfWar.layer = 9;


        foreach (Unit worker in WorldController._worldController._workers)
        {
           CheckForFOW(worker.transform.position, false);
            MonsterVeiw(worker.transform.position);
        }
        foreach (Building building in WorldController._worldController._buildings)
        {
            if (!_BuldingFow.Contains(building))
            {
                CheckForFOW(building.transform.position,true);
                _BuldingFow.Add(building);
            }
            MonsterVeiw(building.transform.position);
        }
        UpdateColour();
        _fogOfWar.layer = 2;
        count = 0;
    }
    void UpdateColour()
    {
  
        _mesh.colors = _colours;
    }

    /// <summary>
    /// Check each vertex around the viewer and will change them to be the right alpher blend
    /// </summary>
    /// <param name="viewer"> The location of the GameObject that intracts with the FOW </param>    
    /// <param name="bulidng"> Checks if the veiwer is a building or not </param>    
    void CheckForFOW(Vector3 viewer, bool building)
    {
        Vector3 FOWDectection = new Vector3(viewer.x, _fogOfWar.transform.position.y + 5, viewer.z);

        Ray r = new Ray(FOWDectection, viewer - FOWDectection);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, _fogLayer, QueryTriggerInteraction.Collide))
        {
            int thisVertex = _mesh.triangles[hit.triangleIndex * 3];

            int y = 0;


            int start = thisVertex - (_yDiffrent * 2) - _xDiffrent;
            int end = thisVertex + (_yDiffrent * 2) + _xDiffrent;

            for (int i = start; i < end;)
            {
                if (i >= 0 && i < _vertices.Length)
                {
                    count++;

                    Vector3 v = _fogOfWar.transform.TransformPoint(_vertices[i]);
                    float distance = Vector3.SqrMagnitude(v - hit.point);

                    float toX = 7f;
                    float toZ = 7f;

                    if (v.x > viewer.x)
                    {
                        toX *= -1;
                    }
                    if (v.z > viewer.z)
                    {
                        toZ *= -1;
                    }

                    if (distance < RadiusSqr && !Physics.Linecast(new Vector3(viewer.x, viewer.y, viewer.z), new Vector3(v.x + toX, viewer.y, v.z + toZ)))
                    {
                        //GOING BACK TO FOG DOSN'T WORK!
                        float alpha;
                        if (distance / RadiusSqr > 0.3f && _colours[i].a <= 0.3f && !_changedVectors.Contains(i))
                        {
                            alpha = 0.3f;
                        }
                        else
                        {
                            alpha = Mathf.Min(_colours[i].a, distance / RadiusSqr);
                        }


                        _colours[i].a = alpha;
                        _changedVectors.Add(i);
                        if(building)
                        {
                            _changedVectorBuildings.Add(i);
                        }
                    }
                }
                if (i == thisVertex - _yDiffrent + (_yDiffrent * y) + _xDiffrent)
                {
                    y++;
                    i += _spaceGap - _xDiffrent * 2;
                }
                else
                {
                    i++;
                }

            }
        }
    }
    /// <summary>
    /// Makes the monster visable if someone is close enough
    /// </summary>
    /// <param name="viewer"> The location of the GameObject that intracts with the Monstor </param>    
    void MonsterVeiw(Vector3 viewer)
        { 
        
            // Revel monster if viewer is close
              int monsterNo = 0;
                    foreach (Monster monster in WorldController._worldController._monsters)
                    {

                        if (Vector3.Distance(monster.transform.position, viewer) < _radius)
                        {
                            _monsterVisable.Add(monsterNo);
                        }
                        monsterNo++;
                    }

                    for (int n = 0; n < WorldController._worldController._monsters.Count; n++)
                    {
                        Monster thisMonster = WorldController._worldController._monsters[n];
                        bool visable = _monsterVisable.Contains(n);
                        for (int m = 1; m < thisMonster.GetComponentsInChildren<MeshRenderer>().Length; m++)
                        {
                            thisMonster.GetComponentsInChildren<MeshRenderer>()[m].enabled = visable;
                        }
                    }
        }
    }


