using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientParenter : MonoBehaviour
{
    TileMap3D _theTilemap;
    float _ickleLittleTimer = 1;

    // Start is called before the first frame update
    void Start()
    {
        _theTilemap = WorldController.GetWorldController.GetComponent<TileMap3D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_ickleLittleTimer <= 0)
        {
            ParentToTile();
            Destroy(this);
        }
        else
        {
            _ickleLittleTimer -= Time.deltaTime;
        }
    }
    void ParentToTile()
    {
        Tile currentTile = _theTilemap.FindCell(transform.position);
        if (currentTile == null)
        {
            return;
        }
        else if(transform.parent != null)
        {
            if (transform.parent.tag == "floor")
            {
                return;
            }
        }       
        
            transform.parent = currentTile.transform;
        
    }
}
