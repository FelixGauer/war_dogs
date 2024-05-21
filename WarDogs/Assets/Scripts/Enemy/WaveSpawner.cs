using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveSpawner : MonoBehaviour
{
    [Header("WaveSystem")]
    public int enemiesAlive = 0;
    public int numberOfEnemies = 0;
    public GameObject[] spawnPoints;
    public GameObject[] enemyPrefabs;
    private GameObject spawnPoint;

    
    [Header("EnemyAI")]
    public EnemyAIScriptableObject[] enemyAiScriptable; //0:Assault, 1:Crawler, 2:Sniper, 3:Boss
    private int totalEnemyTypes;
    
    // public TextMeshProUGUI roundNum;
    public TextMeshProUGUI roundsSurvived; //In End

    private void Awake()
    {
        totalEnemyTypes = enemyAiScriptable.Length;
    }

    void Update() {
        
        if (enemiesAlive == 0) {
            numberOfEnemies++;
            NextWave(numberOfEnemies);
            // roundNum.text = "Round: " + round.ToString();
        }
    }

    public void NextWave(int round) {
        for (int i = 0; i < round; i++) {
            
            //Find active spawn pointer
            while (true) {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (spawnPoint.activeInHierarchy) {
                    break;
                }
            }

            GameObject selectEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            int selectedEnemyType = Random.Range(0, totalEnemyTypes);
            selectEnemy.GetComponent<EnemyAi>().enemyType = enemyAiScriptable[selectedEnemyType];
            GameObject enemySpawned = Instantiate(selectEnemy, spawnPoint.transform.position, Quaternion.identity);
            enemiesAlive++;
        }
    }
}
