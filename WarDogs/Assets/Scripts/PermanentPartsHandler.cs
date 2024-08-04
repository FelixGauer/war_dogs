using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermanentPartsHandler : MonoBehaviour
{
    
    public float health;
    public bool isDestroyed = false;
    private float damage;
    
    void Update()
    {
        if(health <= 0)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet") && health >= -1f)
        {
            // damage = other.GetComponent<BulletHandler>().damage;
            // health -= damage;
        }
    }
}
