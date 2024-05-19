using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAi : MonoBehaviour
{
    //Increase speed of some enemies on chse state using navmesh agent component 
    
    public NavMeshAgent agent;
    public EnemyAIScriptableObject enemyType;
    
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

    private void Awake()
    {
        // player = GameObject.Find("Player").transform; //for multiplayer do randome.range and maybe use tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyType.speed;
        SearchWalkPoint();
    }

    private void Update()
    {
        //Check sight and range
        playerInSightRange = Physics.CheckSphere(transform.position, enemyType.sightRange, whatIsPlayer);

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
        Debug.Log("1234");
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
        enemyType.sightRange = enemyType.sightRange + enemyType.increaseSightOnGettingAttacked; //Maybe keep it maybe not
        
        enemyType.health -= damage;

        if (enemyType.health <= 0)
        {
             Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        Debug.Log("Dead");
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyType.attackRange);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, enemyType.sightRange);
    }
}
