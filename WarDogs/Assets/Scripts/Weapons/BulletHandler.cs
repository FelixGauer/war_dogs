using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    public float deactivateTimer;

    private void Update()
    {
        deactivateTimer -= Time.deltaTime;
        
        if (deactivateTimer <= 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    //Use these to method to get different explosion method. Eg - if one wanna have different particle system when colliding with enemy

    private void OnTriggerEnter(Collider other)
    {
        this.gameObject.SetActive(false);
    }

    private void OnParticleCollision(GameObject other)
    {
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        deactivateTimer = 3f;
    }
}
