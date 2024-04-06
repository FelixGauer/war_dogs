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

    [Header("Bullet Object")]
    public int bulletObjectAmount;
    public List<GameObject> bulletObjectPool;
    public GameObject bulletObjectPrefab;
    public GameObject bulletObjectContainer;
    private void Awake()
    {
        instance = this;
        
        BulletTrailPooling();

        BulletObjectPooling();
    }


    #region Bullet Trail Pooling
    
    public GameObject GetPooledBulletsTrails()
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
    private void BulletTrailPooling()
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
    
    
    #region Bullet Object Pooling
    
    public GameObject GetPooledBulletObject()
    {
        for (int i = 0; i < bulletObjectAmount; i++)
        {
            if (!bulletObjectPool[i].activeInHierarchy)
            {
                return bulletObjectPool[i];
            }
        }

        GameObject tmp;
        tmp = Instantiate(bulletObjectPrefab);
        tmp.transform.SetParent(bulletObjectContainer.transform);
        bulletObjectPool.Add(tmp);
        bulletObjectAmount++;
        return tmp;
    }
    private void BulletObjectPooling()
    {
        bulletObjectPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletObjectAmount; i++)
        {
            tmp = Instantiate(bulletObjectPrefab);
            tmp.transform.SetParent(bulletObjectContainer.transform);
            tmp.SetActive(false);
            bulletObjectPool.Add(tmp);
        }
    }
    #endregion
}
