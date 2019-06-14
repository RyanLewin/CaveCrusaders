using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawnTile : Tile
{
    [SerializeField] List<GameObject> _monsterList = new List<GameObject>();
    [SerializeField] List<AudioClip> _aMonsterHasAppired = new List<AudioClip>();


    protected override void OnTileStart()
    {
  //      WorldController.GetWorldController._aiSpawnTiles.Add(this);
    }

    public override int GetID()
    {
        return (int)TileTypeID.AISpawn;
    }

    public void SpawnMonster()
    {
        if (WorldController.IsPlaying())
        {

            if (_monsterList.Count > 0)
            {
                int monsterNum = Random.Range(0, _monsterList.Count);
                if(_monsterList[monsterNum].GetComponent<MonsterSlug>() != null)
                {
                    Instantiate(_monsterList[monsterNum], transform.position, new Quaternion(0, 0, 0, 0));
                    Instantiate(_monsterList[monsterNum], transform.position, new Quaternion(0, 0, 0, 0));
                }

            Instantiate(_monsterList[monsterNum], transform.position, new Quaternion(0, 0, 0, 0));
                if (_aMonsterHasAppired.Count != 0)
                {
                    //WorldController._worldController._voiceOverAudioSource.clip = _aMonsterHasAppired[Random.Range(0, _aMonsterHasAppired.Count)];
                //    WorldController._worldController._voiceOverAudioSource.Play();
                }
            }
        }
    }
}
