using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;
    
    [Header("BulletsTrail")]
    public int bulletTrainAmount;
    public List<GameObject> bulletTrailPool;
    public GameObject bulletTrailPrefab;
    public GameObject bulletTrailContainer;

    private void Awake()
    {
        instance = this;
        
        bulletTrailPooling();
    }


    #region Bullet Trail Pooling
    
    public GameObject GetPooledBullets()
    {
        for (int i = 0; i < bulletTrainAmount; i++)
        {
            if (!bulletTrailPool[i].activeInHierarchy)
            {
                return bulletTrailPool[i];
            }
        }

        GameObject tmp;
        tmp = Instantiate(bulletTrailPrefab);
        tmp.transform.SetParent(bulletTrailContainer.transform);
        bulletTrailPool.Add(tmp);
        bulletTrainAmount++;
        return tmp;
    }
    private void bulletTrailPooling()
    {
        bulletTrailPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletTrainAmount; i++)
        {
            tmp = Instantiate(bulletTrailPrefab);
            tmp.transform.SetParent(bulletTrailContainer.transform);
            tmp.SetActive(false);
            bulletTrailPool.Add(tmp);
        }
    }
    #endregion
}
