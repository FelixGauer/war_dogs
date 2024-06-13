using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("PlayerStats")] 
    public float maxHealth;
    public float health = 120;
    public bool allowToHeal;
    public float damage;
    public float respawnTimer;
    public float respawnTimeIncrease;
    
    [Header("References")]
    public FirstPersonController fpsControllerScript;
    public Guns gunsScript;
    public GameManager gameManager;

    private void Awake()
    {
        fpsControllerScript = GetComponentInParent<FirstPersonController>();
        gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
        gameManager.currentPlayer++; //Increase player amount
    }

    private void Update()
    {
        if (health <= maxHealth && allowToHeal)
        {
            health += Time.deltaTime / 2;
        }

        if (health <= 0)
        {
            allowToHeal = false;
            Debug.Log("Dead");
            fpsControllerScript.enabled = false;
            gunsScript.enabled = false;
            gameManager.currentAlivePlayers--; //Fix this
            Respawn();
            //Kill Player
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet") && health >= -1f)
        {
            Debug.Log("damageTaken");
            gameManager.currentAlivePlayers++;
            allowToHeal = false;
            damage = other.GetComponent<BulletHandler>().damage;
            health -= damage;
            StartCoroutine(HealthBool());
        }
    }
    
    private IEnumerator HealthBool()
    {
        yield return new WaitForSeconds(5f);
        allowToHeal = true;
    }

    private void Respawn()
    {
        respawnTimer -= Time.deltaTime;
        
        if(respawnTimer <= 0)
        {
            fpsControllerScript.enabled = true;
            gunsScript.enabled = true;
            respawnTimer =+ respawnTimeIncrease;
            health = maxHealth;
            allowToHeal = true;
        }
    }
}
