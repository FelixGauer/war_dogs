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
    
    [Header("Testing")] //Remove after model impliementation
    public Material groundEnemyMaterial;
    public Material flyingEnemyMaterial;
    
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
        // Find GameObjects with Players
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
        
        // Find GameObjects with PermanentParts
        GameObject[] permanentPartsObjectsArray = GameObject.FindGameObjectsWithTag("PermanentPart");
        foreach (GameObject permanentPartObject in permanentPartsObjectsArray)
        {
            players.Add(permanentPartObject.transform);
        }
        
        if (players.Count > 0)
        {
            player = players[Random.Range(0, players.Count)];
        }
        
        agent = GetComponent<NavMeshAgent>();
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
            agent.agentTypeID = -1372625422; //Flying Enemy ID found from Inspector Debug
            agent.baseOffset = baseOffset;
        }
    }

    private void Update()
    {
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
        
        //If gameObject is null, it removes it from the list and switch target to another player
        if(player == null)
        {
            for (int i = players.Count - 1; i >= 0; i--)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                }
            }
            
            Debug.Log(player);
            player = players[Random.Range(0, players.Count)];
            agent.SetDestination(player.position);
        }
        
        EnemyTypeCondition();
    }
    
    private void ChasePlayer()
    {
        Debug.Log("Chasing Player");
        
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
            bool hasIncreasedTransform = false;
            
            //Remove this once models are impliemented
            if (!hasIncreasedTransform)
            {
                transform.localScale = new Vector3(3, 3, 3);
                hasIncreasedTransform = true;
            }
            
            Debug.Log("Boss Enemy");
            float healthThreshold = enemyType.health * bossHealthThreshold;

            if (health <= healthThreshold && !hasIncreasedSpeed)
            {
                agent.speed += increaseBossSpeed;
                Debug.Log(agent.speed + " " + increaseBossSpeed);
                hasIncreasedSpeed = true;
            }
        } 
        
        bool hasChangedMaterial = false;
        if (!hasChangedMaterial)
        {
            if (enemyType.isGroundEnemy)
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material = groundEnemyMaterial;
            }
            else
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material = flyingEnemyMaterial;
            }

            hasChangedMaterial = true;
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
