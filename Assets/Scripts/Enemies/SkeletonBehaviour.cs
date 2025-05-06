using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonBehaviour : MonoBehaviour, IEnemy
{
    [SerializeField] private int maxHealth;
    [SerializeField] private float movingSpeed;
    [SerializeField] private float minDistance;
    [Space]
    [SerializeField] private int damageAmount;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDistance;
    [Space]
    [SerializeField] private int score;
    [Space]
    [SerializeField] private Animator animator;
    [SerializeField] private float destroyTime;

    private NavMeshAgent navMeshAgent;
    private MainPlayer player;

    private int currentHealth;
    private bool isAttacking;

    public event Action<IEnemy> EnemyDied;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = FindFirstObjectByType<MainPlayer>();
        navMeshAgent.speed = movingSpeed;
        currentHealth = maxHealth;
        isAttacking = false;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > minDistance)
        {
            navMeshAgent.SetDestination(player.transform.position);
        }

        if (distance <= attackDistance)
        {
            if (!isAttacking)
                StartCoroutine(AttackCoroutine());
        }

        transform.LookAt(player.transform);
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.z = 0;
        currentRotation.x = 0;

        transform.eulerAngles = currentRotation;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0 || damageAmount <= 0)
            return;

        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            animator.SetTrigger("DAMAGE");
        }
    }


    public IEnumerator AttackCoroutine()
    {
        animator.SetTrigger("ATTACK");

        isAttacking = true;
        HitPlayer();
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }

    public void HitPlayer()
    {
        player.PlayerHealthManager.ReduceHealth(damageAmount);
    }

    public void Die()
    {
        animator.SetTrigger("DEATH");
        navMeshAgent.isStopped = true;
        isAttacking = true;

        player.ScoreManager.AddScore(score);

        EnemyDied?.Invoke(this);

        Invoke("DeleteObject", destroyTime);
    }

    void DeleteObject()
    {
        Destroy(gameObject);
    }
}
