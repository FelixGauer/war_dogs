using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallHealth : MonoBehaviour
{
    public float health;
    private Material wallMat;
    public float timeCounter;
    public float time;
    public float damage;
    public GameObject spawnPoint;
    private void Awake()
    {
        wallMat = GetComponent<Renderer>().material;
        time = Random.Range(0, 15);
    }
    
    void Update()
    {
        timeCounter += Time.deltaTime;

        if (timeCounter >= time)
        {
            damage = Random.Range(0, 15);
            health -= damage;
            time = Random.Range(0, 15);
            timeCounter = 0f;
        }

        if (health <= 0f)
        {
            wallMat.color = Color.red;
            spawnPoint.SetActive(true);
        }
    }
    
}
