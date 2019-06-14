using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] WorkerSound _workerSound;
    [SerializeField] Worker _worker;
   public void Picked()
    {
        _workerSound.PlayPickSound();
        if (_worker.CurrentTask._targetRock != null)
        {
            _worker.CurrentTask._targetRock.PickaxeMined();
        }
    }
    
    public void Footstep()
    {
        _workerSound.PlayFootstep();
    }
    public void Dig()
    {
        _workerSound.PlayShovelSound();
    }
    public void Shoot()
    {
        if (_worker._closestMonster != null)
        {
            _workerSound.PlayLaserSound();
            GameObject temp = Instantiate(_worker._bulletPrefab, _worker._currentToolModel.transform.GetChild(0).transform.position, new Quaternion());
            temp.GetComponent<LaserBullet>().SetTarget(_worker._closestMonster.transform.position);
        }
    }
    public void Drill()
    {
        _workerSound.PlayDrillSound();
    }
    public void FireBulletWithAnimation()
    {
        _workerSound.PlayLaserSound();
        
    }
}
