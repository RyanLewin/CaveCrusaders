using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldFOW : MonoBehaviour
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
    float RadiusSqr { get { return _radius * _radius; } }

    int count = 0;

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
    

    }

    // Update is called once per frame
    void Update()
    {
        _changedVectors.Clear();
        _monsterVisable.Clear();

        _fogOfWar = WorldController._worldController._fogOfWar;
        WorldController world = WorldController._worldController;
        _fogOfWar.layer = 9;
        foreach (Unit worker in WorldController._worldController._workers)
        {
            CheckForFOW(worker.transform.position);
        }
        foreach (Building building in WorldController._worldController._buildings)
        {
            CheckForFOW(building.transform.position);
        }
        _fogOfWar.layer = 2;
    }
    void UpdateColour()
    {
        _mesh.colors = _colours;
    }


    void CheckForFOW(Vector3 viewer)
    {      
        Vector3 FOWDectection = new Vector3(viewer.x, _fogOfWar.transform.position.y + 1, viewer.z);

        Ray r = new Ray(FOWDectection, viewer - FOWDectection);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, _fogLayer, QueryTriggerInteraction.Collide))
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                count++;
                Vector3 v = _fogOfWar.transform.TransformPoint(_vertices[i]);
                float distance = Vector3.SqrMagnitude(v - hit.point);

                float toX = 2.5f;
                float toZ = 2.5f;

                if(v.x > viewer.x)
                {
                    toX *= -1;
                }
                if (v.z > viewer.z)
                {
                    toZ *= -1;
                }

                if (distance < RadiusSqr && !Physics.Linecast(new Vector3(viewer.x, viewer.y + 3, viewer.z), new Vector3(v.x+toX, viewer.y + 3, v.z+toZ)))
                {

                    float alpha = Mathf.Min(_colours[i].a, distance / RadiusSqr);
                    if (alpha <= 0.2f)
                    {
                        alpha = 0;
                    }
                    _colours[i].a = alpha;
                    _changedVectors.Add(i);

                }
                else if (_colours[i].a < 0.2f)
                {


                    if (!_changedVectors.Contains(i))
                    {
                        _colours[i].a = 0.2f;
                    }

                }
            }
            UpdateColour();
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
}

