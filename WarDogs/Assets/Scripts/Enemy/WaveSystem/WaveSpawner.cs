using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance;
    
    [Header("WaveSystem")] 
    public GameObject enemyPrefab;
    private GameObject spawnPoint;
    public GameObject enemyGo;
    private bool waveIncremented = false;
    private bool randomiseSpawn;

    [Header("NotControlledValues - Change to Private in future")]
    public int maxEnemies;
    public int enemiesAlive = 0;
    public int enemiesToSpawn;
    public int wave = 1;
    public GameObject[] spawnPoints;
    public List<GameObject> activeSpawnPoints = new List<GameObject>();

    [Header("Boss Wave Settings")]
    public EnemyAIScriptableObject bossEnemy; //Change to array if more bosses are introduced
    public int reduceSpawnByDivision = 2; //divide the spawn by this number to reduce the number of enemies spawned
    
    [Header("EnemySpawning")]
    public int additionalMaxEnemies = 2; //Gamedesigners vars
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
        totalEnemyTypes = enemyAiScriptable.Length;
    }

    void Update() {
        
        // enemiesLeft.text = "Enemies Left: " + spawnedEnemies.Count.ToString(); //Display number of enemies left
        
        if (enemiesAlive <= minEnemies)
        {
            if (!waveIncremented)
            {
                wave++;
                waveNum.text = "Wave: " + wave.ToString(); //Display number of waves
                waveIncremented = true;
            }

            if (wave % 10 == 0 && activeSpawnPoints.Count >= 12) //if active spawn points are more than 50% boss wave 
            {
                randomiseSpawn = false;
                SpawnEnemies(enemiesToSpawn / reduceSpawnByDivision , bossEnemy);
                return;
            }
            else if (wave % 5 == 0) //Particular Enemy Wave
            {
                SpawnEnemies(enemiesToSpawn, enemyAiScriptable[Random.Range(0, totalEnemyTypes)]);
                randomiseSpawn = false;
                return;
            }
            else //Default Wave
            {
                randomiseSpawn = true;
                SpawnEnemies(enemiesToSpawn, enemyAiScriptable[0]);
                return;
            }
        }
        else
        {
            waveIncremented = false;
        }
    }
    
    public void SpawnEnemies(int numberOfEnemies, EnemyAIScriptableObject enemyType)
    {

        maxEnemies = maxEnemies + additionalMaxEnemies;
        enemiesToSpawn = minEnemies + (wave * additionalEnemiesPerWave) + (activeSpawnPoints.Count * additionalEnemiesPerBreaches);

        //I'm thinking the biggest problem is how the brakets are facorized. Instead I'm thinking:
        //- large inital addition to min enemies (can be done already)
        //- the system should take into account how many enemies are still left
        //- additional breaches and waev progression should work additively
        //      that could be achieved with:
        //          - additionalEnemiesper Breach/Wave are either 1 or > 1 and rounding up. Keeping this is a factor is still smart, if we want to ge below 1
        //- use maxenemies as the hardcap for performance reasons so no +additionalmaxenemies
        //- add another variable instead, let's call it optimalEnemies
        //  optimalEnemies = min + (wave * additionalEnemiesPerWave) + (activeSpawnPoints.Count * additionalEnemiesPerBreaches))
        //- enemiestospawn = (optimalEnemies - enemiesstillleft) * additional factor of >1 so there is always a dynamic spawn

        if (enemiesToSpawn >= maxEnemies) 
        {
            enemiesToSpawn = maxEnemies;
        }
        
        for (int i = 0; i < numberOfEnemies; i++)
        {

            enemyGo = PoolManager.instance.GetPooledEnemy();
            if(enemyGo != null && activeSpawnPoints.Count > 0) {

                if (randomiseSpawn)
                {
                    enemyType = enemyAiScriptable[Random.Range(0, totalEnemyTypes)];
                }

                enemyGo.GetComponent<EnemyAi>().enemyType = enemyType;
                Debug.Log(enemyType.name);

                spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];

                enemyGo.transform.position = spawnPoint.transform.position;
                enemyGo.transform.rotation = spawnPoint.transform.rotation;
                enemyGo.SetActive(true);
                enemiesAlive++;
            }
        }
    }
}
