using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehaviour : MonoBehaviour, IEnemy
{
    [SerializeField] private int maxHealth;
    [SerializeField] private float movingSpeed;
    [SerializeField] private int movingSpeedPercentVariation;
    [SerializeField] private float minDistance; 
    [Space]
    [SerializeField] private int damageAmount;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackDistance;
    [Space]
    [SerializeField] private GameObject healthPickup;
    [SerializeField] private GameObject ammoUziPickup;
    [SerializeField] private GameObject ammoShotgunPickup;
    [Space]
    [SerializeField] private float healthProbability;
    [SerializeField] private float ammoUziProbability;
    [SerializeField] private float ammoShotgunProbability;
    [Space]
    [SerializeField] private int score;
    [Space]
    [SerializeField] private Animator animator;
    [SerializeField] private float destroyTime;

    private MainPlayer player;
    private int currentHealth;
    private bool isAttacking;
    private GameObject objectToInstantiate;

    public event Action<IEnemy> EnemyDied;

    void Start()
    {
        float randomMultiplyer = UnityEngine.Random.Range(100, 100 + movingSpeedPercentVariation);
        movingSpeed = movingSpeed * randomMultiplyer / 100;
        player = FindFirstObjectByType<MainPlayer>();
        currentHealth = maxHealth;
        isAttacking = false;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > minDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, movingSpeed * Time.deltaTime);
        }

        if (distance <= attackDistance)
        {
            if (!isAttacking)
                StartCoroutine(AttackCoroutine());
        }
        transform.LookAt(player.transform);
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
        movingSpeed = 0;
        isAttacking = true;

        int totalProbability = 100;
        int randomValue = UnityEngine.Random.Range(0, totalProbability + 1);
        if (randomValue < healthProbability)
        {
            objectToInstantiate = healthPickup;
        }
        else if (randomValue < healthProbability + ammoUziProbability)
        {
            objectToInstantiate = ammoUziPickup;

        }
        else if (randomValue < healthProbability + ammoUziProbability + ammoShotgunProbability)
        {
            objectToInstantiate = ammoShotgunPickup;
        }
        player.ScoreManager.AddScore(score);

        EnemyDied?.Invoke(this);
        Invoke("DestroyGhost", destroyTime);
    }

    void DestroyGhost()
    {
        if (objectToInstantiate != null)
            Instantiate(objectToInstantiate, transform.position, transform.rotation);
        Destroy(gameObject);
    }


}
