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
    public int enemiesAlive = 0;
    public int wave = 0;
    public int IntensityOfSpawn = 0; //incrementing 1 increments 5-7 enemies per wave
    public GameObject[] spawnPoints;
    public GameObject enemyPrefab;
    private GameObject spawnPoint;
    public GameObject enemyGo;
    
    [Header("References")]
    public TextMeshProUGUI waveNum;
    public TextMeshProUGUI enemiesLeft;
    
    [Header("EnemyAI")]
    public EnemyAIScriptableObject[] enemyAiScriptable; //0:Assault, 1:Crawler, 2:Sniper, 3:Boss
    private int totalEnemyTypes;
    
    
    // public TextMeshProUGUI roundNum;
    public TextMeshProUGUI roundsSurvived; //In End

    private void Awake()
    {
        instance = this;
        totalEnemyTypes = enemyAiScriptable.Length;
    }

    void Update() {
        
        enemiesLeft.text = "Enemies Left: " + enemiesAlive.ToString();
        
        if (enemiesAlive == 0) {
            IntensityOfSpawn += Mathf.CeilToInt(IntensityOfSpawn / 3.0f);
            NextWave(IntensityOfSpawn);
            // roundNum.text = "Round: " + round.ToString();
        }
    }

    public void NextWave(int round)
    {
        wave++;
        waveNum.text = "Wave: " + wave.ToString();
        // Calculate the number of each type of enemy to spawn
        int numAssault = Mathf.CeilToInt(round * 0.60f);
        int numCrawler = Mathf.CeilToInt(round * 0.20f);
        int numSniper = Mathf.CeilToInt(round * 0.10f);
        int numBoss = Mathf.CeilToInt(round * 0.10f);

        List<int> enemiesToSpawn = new List<int>();

        enemiesToSpawn.AddRange(Enumerable.Repeat(0, numAssault));
        enemiesToSpawn.AddRange(Enumerable.Repeat(1, numCrawler));
        enemiesToSpawn.AddRange(Enumerable.Repeat(2, numSniper));
        enemiesToSpawn.AddRange(Enumerable.Repeat(3, numBoss));

        // Shuffle the list to randomize enemy order
        enemiesToSpawn = enemiesToSpawn.OrderBy(x => Random.value).ToList();

        
        foreach (int enemyType in enemiesToSpawn) {
            
            enemyGo = PoolManager.instance.GetPooledEnemy();
            enemyGo.GetComponent<EnemyAi>().enemyType = enemyAiScriptable[enemyType];
            // Find active spawn pointer
            while (true) {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (spawnPoint.activeInHierarchy) {
                    break;
                }
            }
            
            SpawnEnemies(IntensityOfSpawn, enemyAiScriptable[enemiesToSpawn[0]]);

            // enemyPrefab.GetComponent<EnemyAi>().enemyType = enemyAiScriptable[enemyType];
            // GameObject enemySpawned = Instantiate(enemyPrefab, spawnPoint.transform.position, Quaternion.identity);
            // enemiesAlive++;
        }
    }

    

    public List<GameObject> SpawnEnemies(int numberOfEnemies, EnemyAIScriptableObject enemyType)
    {
        List<GameObject> spawnedEnemies = new List<GameObject>();

        for (int i = 0; i < numberOfEnemies; i++)
        {
            enemyGo = PoolManager.instance.GetPooledEnemy();

            
            if(enemyGo != null) {
                enemyPrefab.GetComponent<EnemyAi>().enemyType = enemyType;
                enemyGo.transform.position = spawnPoint.transform.position;
                enemyGo.transform.rotation = spawnPoint.transform.rotation;
                enemyGo.SetActive(true);
                spawnedEnemies.Add(enemyGo);
                enemiesAlive++;
            }
        }

        return spawnedEnemies;
    }
}
