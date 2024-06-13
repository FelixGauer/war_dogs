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
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public GameObject enemyBulletGo;
    
    [Header("Patrol")]
    public Vector3 walkPoint;
    public bool walkPointSet;
    public Vector3 tempWalkPoint;
    
    [Header("Attack")]
    private bool alreadyAttacked;
    
    [Header("About States")]
    public bool playerInSightRange;
    public bool playerInAttackRange;

    [Header("ScriptableObjectReferences")] 
    public float health;
    public float sightRange;
    public float attackRange;
    public float speed;
    public float increaseSpeedOnGettingAttacked;
    
    private void Awake()
    {
        // player = GameObject.Find("Player").transform; //for multiplayer do randome.range and maybe use tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        SearchWalkPoint();
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
        //Check sight and range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (enemyType.doesEnemyPatrol)
        {
            if (playerInSightRange)
            {
                playerInAttackRange = Physics.CheckSphere(transform.position, enemyType.attackRange, whatIsPlayer);
            }

            if (!playerInSightRange && !playerInAttackRange)
            {
                Patrolling();
            }

            if (playerInSightRange && !playerInAttackRange)
            {
                ChasePlayer();
                tempWalkPoint = player.position;
            }

            if (playerInAttackRange && playerInSightRange)
            {
                AttackPlayer();
            }
        }
        else
        {
            ChasePlayer();
            
            if (playerInSightRange)
            {
                playerInAttackRange = Physics.CheckSphere(transform.position, enemyType.attackRange, whatIsPlayer);
            }
            
            if (playerInAttackRange && playerInSightRange)
            {
                AttackPlayer();
            }
        }
    }

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
    
    private void ChasePlayer()
    {
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

        if (health <= 0)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
