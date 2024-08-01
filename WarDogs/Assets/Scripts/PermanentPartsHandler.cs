using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermanentPartsHandler : MonoBehaviour
{
    
    public float health;
    private float damage;
    
    void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet") && health >= -1f)
        {
            Debug.Log("DAMAGE TAKEN");
            damage = other.GetComponent<BulletHandler>().damage;
            health -= damage;
        }
    }
}
