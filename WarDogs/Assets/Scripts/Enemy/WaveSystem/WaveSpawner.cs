using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;
    
    [Header("WaveSystem")] 
    public GameObject enemyPrefab;
    private GameObject spawnPoint;
    public GameObject enemyGo;
    
    [Header("NotControlledValues - Change to Private in future")]
    public int maxEnemies;
    public int enemiesAlive = 0;
    public int enemiesToSpawn;
    public int wave = 1;
    public GameObject[] spawnPoints;
    public List<GameObject> activeSpawnPoints = new List<GameObject>();

    [Header("EnemySpawning")]
    public int additionalMaxEnemies = 2;
    public int minEnemies = 2;
    public int additionalEnemiesPerWave = 2;
    public int additionalEnemiesPerBreaches = 2;

    [Header("References")]
    public TextMeshProUGUI waveNum; //These are for us to check the wave number
    public TextMeshProUGUI enemiesLeft; //These are for us to check the number of enemies left
    
    [Header("EnemyAI")]
    public EnemyAIScriptableObject[] enemyAiScriptable; //0:Assault, 1:Crawler, 2:Sniper
    private int totalEnemyTypes;

    private void Awake()
    {
        instance = this;
        totalEnemyTypes = enemyAiScriptable.Length; // 4, calculates enemy types and put it in int
    }

    void Update() {
        
        // enemiesLeft.text = "Enemies Left: " + spawnedEnemies.Count.ToString(); //Display number of enemies left
        
        if (enemiesAlive <= minEnemies) 
        {
            Debug.Log("1");
            SpawnEnemies(enemiesToSpawn, enemyAiScriptable[Random.Range(0, totalEnemyTypes)]);
        }
    }
    
    public void SpawnEnemies(int numberOfEnemies, EnemyAIScriptableObject enemyType)
    {
        wave++;
        waveNum.text = "Wave: " + wave.ToString(); //Display number of waves
        maxEnemies = maxEnemies + additionalMaxEnemies;
        enemiesToSpawn = minEnemies + (wave * additionalEnemiesPerWave) + (activeSpawnPoints.Count * additionalEnemiesPerBreaches);
        
        if(enemiesToSpawn >= maxEnemies) 
        {
            enemiesToSpawn = maxEnemies;
        }
        
        Debug.Log("2");
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Debug.Log("Spawning Enemy");

            enemyGo = PoolManager.instance.GetPooledEnemy(); // Get the enemy from the pool instead of Instantiating
            Debug.Log("3");
            if(enemyGo != null && activeSpawnPoints.Count > 0) {
                // Select a random EnemyAIScriptableObject
                enemyType = enemyAiScriptable[Random.Range(0, totalEnemyTypes)];

                enemyGo.GetComponent<EnemyAi>().enemyType = enemyType; // Set the enemyAI script
                Debug.Log(enemyType.name);

                spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];

                enemyGo.transform.position = spawnPoint.transform.position;
                enemyGo.transform.rotation = spawnPoint.transform.rotation;
                enemyGo.SetActive(true); // Set the enemy to active
                enemiesAlive++; // Increment the number of enemies alive
            }
        }
    }
}
