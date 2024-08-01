using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAi : MonoBehaviour
{
    //Increase speed of some enemies on chse state using navmesh agent component 
    
    [Header("General")]
    public EnemyAIScriptableObject enemyType;
    public NavMeshAgent agent;
    public float baseOffset;
    public Transform player;
    public List<Transform> players;
    public List<PlayerStats> playerStats;

    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public GameObject enemyBulletGo;
    
    // [Header("Patrol")]
    // public Vector3 walkPoint;
    // public bool walkPointSet;
    // public Vector3 tempWalkPoint;
    
    [Header("Attack")]
    public float dodgeSpeed;
    private bool isDodging = false;
    public float dodgeCooldown;
    private float dodgeTimer = 0f;
    private Vector3 dodgePosition;
    private bool alreadyAttacked;
    
    [Header("About States")]
    public bool playerInSightRange;
    public bool playerInAttackRange;

    [Header("Enemy Type Conditions")] 
    public float increaseBossSpeed;
    public float bossHealthThreshold = 0.5f;
    
    [Header("ScriptableObjectReferences")] 
    public float health;
    public float sightRange;
    public float attackRange;
    public float speed;
    public float increaseSpeedOnGettingAttacked;
    
    private void Awake()
    {
        // player = GameObject.Find("Player").transform; //for multiplayer do randome.range and maybe use tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            players.Add(playerObject.transform);
            PlayerStats stats = playerObject.GetComponentInChildren<PlayerStats>();
            if (stats != null)
            {
                playerStats.Add(stats);
            }
        }
        if (players.Count > 0)
        {
            player = players[Random.Range(0, players.Count)];
        }
        agent = GetComponent<NavMeshAgent>();
        // SearchWalkPoint();
    }

    private void Start()
    {
        health = enemyType.health;
        sightRange = enemyType.sightRange;
        attackRange = enemyType.attackRange;
        speed = enemyType.speed;
        increaseSpeedOnGettingAttacked = enemyType.increaseSightOnGettingAttacked;
        agent.speed = speed;
        if (!enemyType.isGroundEnemy)
        {
            Debug.Log("Flying Enemy");
            agent.agentTypeID = -1372625422; //Flying Enemy ID found from Inspector Debug
            agent.baseOffset = baseOffset;
        }
    }

    private void Update()
    {
        Debug.Log(agent.speed);
        
        //dodge
        dodgeTimer += Time.deltaTime;
        if (isDodging)
        {
            transform.position = Vector3.MoveTowards(transform.position, dodgePosition, dodgeSpeed * Time.deltaTime);
            if (transform.position == dodgePosition)
            {
                isDodging = false;
            }
        }
        
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (playerInSightRange)
        {
            playerInAttackRange = Physics.CheckSphere(transform.position, enemyType.attackRange, whatIsPlayer);
        }

        if (!playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }

        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }

        if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
        
        EnemyTypeCondition();
    }

    /*
    private void Patrolling()
    {
        // sightRange = sightRange - increaseSightOnGettingAttacked;
        if (!walkPointSet || tempWalkPoint == this.transform.position)
        {
            SearchWalkPoint();
        }

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        
        //Walkpoint Reached
        if (distanceToWalkPoint.magnitude < 2f)
        {
            walkPointSet = false;
        }
    }
    */

    /*
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-enemyType.walkPointRange, enemyType.walkPointRange);
        float randomX = Random.Range(-enemyType.walkPointRange, enemyType.walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        // {
            walkPointSet = true;
        // }
    }
    */
    
    private void ChasePlayer()
    {
        PlayerStats playerStatsScript = player.GetComponentInChildren<PlayerStats>();

        if (playerStatsScript != null)
        {
            if (playerStatsScript.health <= 0)
            {
                if (players.Count > 0)
                {
                    player = players[Random.Range(0, players.Count)];
                    // Get the PlayerStats script from the new player
                    playerStatsScript = player.GetComponentInChildren<PlayerStats>();
                    
                    if(playerStatsScript.health <= 0)
                    {
                        player = players[Random.Range(0, players.Count)];
                    }
                }
            }
        }
        
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // AttackCode
            Debug.Log("ATTACKED");
            
            enemyBulletGo = PoolManager.instance.GetPooledEnemyBulletObject();
            if (enemyBulletGo != null)
            {
                enemyBulletGo.transform.position = this.transform.position;
                enemyBulletGo.transform.rotation = this.transform.rotation;
                enemyBulletGo.SetActive(true);
            }

            Rigidbody rb = enemyBulletGo.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * enemyType.throwSpeed, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), enemyType.timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        //Increase stats on getting attacked
        sightRange = sightRange + enemyType.increaseSightOnGettingAttacked;
        speed = speed + increaseSpeedOnGettingAttacked;
        
        health -= damage;

        if (health > 0 && dodgeTimer >= dodgeCooldown)
        {
            // Dodge the next attack
            int dodgeDirection = (Random.Range(0, 2) * 2 - 1) * 2; // Generates either -2 or 2
            Vector3 towardsPlayer = (player.position - transform.position).normalized;
            Vector3 dodgeVector = Vector3.Cross(towardsPlayer, Vector3.up) * dodgeDirection;
            dodgePosition = transform.position + dodgeVector;
            isDodging = true;
            dodgeTimer = 0f;
        }
        
        if(health <= 0)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        Debug.Log("Dead");
        WaveSpawner.instance.enemiesAlive--;
        this.gameObject.SetActive(false);
    }

    private void EnemyTypeCondition()
    {
        //check for boss type enemy
        if (enemyType.isBossEnemy)
        {
            bool hasIncreasedSpeed = false;

            Debug.Log("Boss Enemy");
            float healthThreshold = enemyType.health * bossHealthThreshold;

            if (health <= healthThreshold && !hasIncreasedSpeed)
            {
                agent.speed += increaseBossSpeed;
                Debug.Log(agent.speed + " " + increaseBossSpeed);
                hasIncreasedSpeed = true;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
