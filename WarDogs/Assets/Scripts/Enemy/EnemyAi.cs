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
    private PlayerStats playerStats;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public GameObject enemyBulletGo;
    
    [Header("Testing")] //Remove after model impliementation
    public Material groundEnemyMaterial;
    public Material flyingEnemyMaterial;

    [Header("Attack")] 
    public GameObject firePoint;
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
    
    [Header("ScriptableObjectReferences --- Goes Private in future as well")] 
    public float health;
    public float damage;
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
        }
        
        if (players.Count > 0)
        {
            player = players[Random.Range(0, players.Count)];
        }
        
        if (player != null && player.gameObject.CompareTag("Player"))
        {
            playerStats = player.GetComponentInChildren<PlayerStats>();
        }
        
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        health = enemyType.health;
        damage = enemyType.damage;
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

        
        Collider[] collidersInSightRange = Physics.OverlapSphere(transform.position, sightRange);
        Collider[] collidersInAttackRange = Physics.OverlapSphere(transform.position, attackRange);

        if (!playerInSightRange)
        {
            foreach (var collider in collidersInSightRange)
            {
                if (collider.transform == player)
                {
                    playerInSightRange = true;
                    break;
                }
            }
        }

        if (!playerInAttackRange)
        {
            foreach (var collider in collidersInAttackRange)
            {
                if (collider.transform == player)
                {
                    playerInAttackRange = true;
                    break;
                }
            }
        }

        if (!playerInSightRange && !playerInAttackRange || playerStats.isDead)
        {
            ChasePlayer();
        }

        if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }
        
        EnemyTypeCondition();
    }
    
    private void ChasePlayer()
    {
        if (playerStats != null && playerStats.isDead)
        {
            
            playerInSightRange = false;
            playerInAttackRange = false;
            
            for (int i = players.Count - 1; i >= 0; i--)
            {
                PlayerStats otherPlayerStats = players[i].GetComponentInChildren<PlayerStats>();
                if (otherPlayerStats != null && !otherPlayerStats.isDead)
                {
                    player = players[i];
                    playerStats = otherPlayerStats;
                    break;
                }
            }
        }

        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);
        transform.LookAt(firePoint.transform);

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
            enemyBulletGo.transform.position = firePoint.transform.position;
            Vector3 throwDirection = (player.position - firePoint.transform.position).normalized;
            rb.AddForce(throwDirection * enemyType.throwSpeed, ForceMode.Impulse); 
            
            RaycastHit hit;
            if (Physics.Raycast(firePoint.transform.position, throwDirection, out hit, sightRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerStats>().health -= damage;
                }
            }
            
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
