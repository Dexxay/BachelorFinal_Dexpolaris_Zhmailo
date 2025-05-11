using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOBehaviour : MonoBehaviour, IEnemy
{
    [Header("References")]
    [SerializeField] private LaserWeapon laserWeapon;
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem explosionEffect;

    [Header("Movement & Behavior Settings")]
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float movementSpeedVariation = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float relativeAltitude = 10f;
    [SerializeField] private float relativeAltitudeVariation = 5f;
    [SerializeField] private float altitudeCorrectionForce = 5f;
    [SerializeField] private float altitudeResponsiveness = 5f;
    [SerializeField] private float horizontalMovementResponsiveness = 5f;

    [Header("Detection Settings")]
    [SerializeField] private float playerDetectionRadius = 40f;

    [Header("Flocking Settings")]
    [SerializeField] private float flockDetectionRadius = 20f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 1.0f;
    [SerializeField] private LayerMask ufoLayer;

    [Header("Circling Flocking Overrides")]
    [SerializeField] private float circlingSeparationRadius = 25f;
    [SerializeField] private float circlingSeparationWeight = 3.0f;

    [Header("Circling Settings")]
    [SerializeField] private float circlingDistance = 15f;
    [SerializeField] private float circlingDistanceVariation = 10f;
    [SerializeField] private float circlingSpeedMultiplier = 0.8f;
    [SerializeField] private float circlingRadiusCorrectionWeight = 1.0f;
    [SerializeField] private float circlingTangentialWeight = 2.0f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 50f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float aimAheadFactor = 0.5f;
    [SerializeField] private float aimingTime = 0.5f;

    [Header("Health & Death")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private int deathScoreValue = 100;
    [SerializeField] private float destructionDelay = 2f;

    private int currentHealth;
    private Rigidbody rb;
    private MainPlayer mainPlayerTarget;
    private Transform playerTransform;
    private float lastAttackTime;
    private Vector3 currentTargetPosition;
    private bool isDead = false;
    private Coroutine attackProcessCoroutine;
    private float instanceCirclingDistance;
    private float instanceRelativeAltitude;
    private float instanceMovementSpeed;


    private enum AIState
    {
        Idle,
        ApproachingPlayer,
        CirclingPlayer,
        Attacking,
        Dead
    }
    private AIState currentState = AIState.Idle;

    public event Action<IEnemy> EnemyDied;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("UFO_AI requires a Rigidbody component on " + gameObject.name);
            enabled = false;
            return;
        }
        rb.useGravity = false;

        if (laserWeapon == null)
        {
            laserWeapon = GetComponentInChildren<LaserWeapon>();
            if (laserWeapon == null)
            {
                Debug.LogError("LaserWeapon not assigned or found in children for UFO_AI on " + gameObject.name);
            }
        }
        if (firePoint == null)
        {
            firePoint = laserWeapon != null ? laserWeapon.transform : transform;
        }
        currentHealth = maxHealth;
    }

    void Start()
    {
        mainPlayerTarget = FindFirstObjectByType<MainPlayer>();

        if (mainPlayerTarget != null)
        {
            playerTransform = mainPlayerTarget.transform;

            instanceCirclingDistance = circlingDistance + UnityEngine.Random.Range(-circlingDistanceVariation, circlingDistanceVariation);
            instanceCirclingDistance = Mathf.Max(1f, instanceCirclingDistance);

            instanceRelativeAltitude = relativeAltitude + UnityEngine.Random.Range(-relativeAltitudeVariation, relativeAltitudeVariation);
            instanceRelativeAltitude = Mathf.Max(2f, instanceRelativeAltitude);

            instanceMovementSpeed = movementSpeed + UnityEngine.Random.Range(-movementSpeedVariation, movementSpeedVariation);
            instanceMovementSpeed = Mathf.Max(1f, instanceMovementSpeed);

            currentTargetPosition = GetInitialTargetPosition();
            StartCoroutine(DecisionTreeCoroutine());
        }
        else
        {
            Debug.LogError("Player with MainPlayer component not found in scene! UFO_AI on " + gameObject.name + " needs a player target. UFO will be disabled.");
            currentState = AIState.Idle;
            enabled = false;
            return;
        }

        lastAttackTime = -attackCooldown;
    }

    private Vector3 GetInitialTargetPosition()
    {
        if (playerTransform != null)
        {
            return playerTransform.position;
        }
        return transform.position;
    }

    private IEnumerator DecisionTreeCoroutine()
    {
        while (!isDead)
        {
            AIState nextState = DetermineNextState();

            if (nextState != currentState)
            {
                if (currentState == AIState.Attacking && attackProcessCoroutine != null)
                {
                    StopCoroutine(attackProcessCoroutine);
                    attackProcessCoroutine = null;
                }

                currentState = nextState;

                if (currentState == AIState.Attacking && attackProcessCoroutine == null)
                {
                    attackProcessCoroutine = StartCoroutine(AttackCoroutine());
                }
            }

            yield return null;
        }
    }

    private AIState DetermineNextState()
    {
        if (isDead) return AIState.Dead;
        if (playerTransform == null) return AIState.Idle;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool canAttack = CanAttack();

        switch (currentState)
        {
            case AIState.Idle:
                if (distanceToPlayer < playerDetectionRadius)
                {
                    return AIState.ApproachingPlayer;
                }
                return AIState.Idle;

            case AIState.ApproachingPlayer:
                if (distanceToPlayer <= instanceCirclingDistance * 1.1f)
                {
                    return AIState.CirclingPlayer;
                }
                if (distanceToPlayer > playerDetectionRadius * 1.5f)
                {
                    return AIState.Idle;
                }
                return AIState.ApproachingPlayer;

            case AIState.CirclingPlayer:
                if (canAttack)
                {
                    return AIState.Attacking;
                }
                if (distanceToPlayer > instanceCirclingDistance * 1.2f)
                {
                    return AIState.ApproachingPlayer;
                }
                return AIState.CirclingPlayer;

            case AIState.Attacking:
                return AIState.Attacking;

            case AIState.Dead:
                return AIState.Dead;
        }

        return AIState.Idle;
    }

    bool CanAttack()
    {
        if (isDead || playerTransform == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown;
    }

    void FixedUpdate()
    {
        if (isDead || currentState == AIState.Attacking)
        {
            if (isDead) rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            if (playerTransform == null && currentState != AIState.Attacking)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 2f);
            }
            return;
        }

        if (playerTransform == null)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 2f);
            return;
        }


        HandleMovement();
        MaintainAltitude();
    }

    private void HandleMovement()
    {
        Vector3 baseMovementDirection = Vector3.zero;
        float currentSpeed = instanceMovementSpeed;

        float activeSeparationRadius = flockDetectionRadius;
        float activeSeparationWeight = separationWeight;
        float activeAlignmentWeight = alignmentWeight;
        float activeCohesionWeight = cohesionWeight;


        if (currentState == AIState.CirclingPlayer)
        {
            activeSeparationRadius = circlingSeparationRadius;
            activeSeparationWeight = circlingSeparationWeight;
            currentSpeed *= circlingSpeedMultiplier;
        }

        Vector3 flockingVector = CalculateFlockingVector(activeSeparationRadius, activeSeparationWeight, activeAlignmentWeight, activeCohesionWeight);


        if (playerTransform != null)
        {
            if (currentState == AIState.CirclingPlayer)
            {
                Vector3 playerToUfoHorizontal = transform.position - playerTransform.position;
                playerToUfoHorizontal.y = 0;

                if (playerToUfoHorizontal.sqrMagnitude < 0.1f)
                {
                    playerToUfoHorizontal = Vector3.right;
                }
                playerToUfoHorizontal.Normalize();

                Vector3 desiredCircleHorizontalPos = playerTransform.position + playerToUfoHorizontal * instanceCirclingDistance;
                Vector3 radiusCorrection = (desiredCircleHorizontalPos - transform.position);
                radiusCorrection.y = 0;

                Vector3 tangentialDirection = Vector3.Cross(Vector3.up, playerToUfoHorizontal);

                baseMovementDirection = (radiusCorrection * circlingRadiusCorrectionWeight + tangentialDirection * circlingTangentialWeight);

            }
            else if (currentState == AIState.ApproachingPlayer)
            {
                Vector3 playerHorizontalPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
                baseMovementDirection = (playerHorizontalPos - transform.position);
                baseMovementDirection.y = 0;
            }
        }

        Vector3 combinedSteeringDirection = baseMovementDirection + flockingVector * instanceMovementSpeed * 0.5f;

        Vector3 lookDirection = combinedSteeringDirection;
        if (lookDirection.sqrMagnitude < 1e-6 && playerTransform != null && currentState != AIState.Idle)
        {
            lookDirection = playerTransform.position - transform.position;
            lookDirection.y = 0;
        }

        if (lookDirection.sqrMagnitude > 1e-6)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        Vector3 desiredHorizontalVelocity = Vector3.zero;
        if (combinedSteeringDirection.sqrMagnitude > 1e-6)
        {
            desiredHorizontalVelocity = combinedSteeringDirection.normalized * currentSpeed;
        }

        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            desiredHorizontalVelocity,
            Time.fixedDeltaTime * horizontalMovementResponsiveness
        );

        rb.velocity = new Vector3(newHorizontalVelocity.x, rb.velocity.y, newHorizontalVelocity.z);

        if (currentState == AIState.Idle || combinedSteeringDirection.sqrMagnitude < 1e-6)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), Time.fixedDeltaTime * 2f);
        }
    }

    private Vector3 CalculateFlockingVector(float activeSeparationRadius, float activeSeparationWeight, float activeAlignmentWeight, float activeCohesionWeight)
    {
        Vector3 separationSum = Vector3.zero;
        Vector3 alignmentSum = Vector3.zero;
        Vector3 cohesionSum = Vector3.zero;
        int alignmentCohesionNeighborsCount = 0;

        float maxDetectionRadius = Mathf.Max(flockDetectionRadius, activeSeparationRadius);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, maxDetectionRadius, ufoLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;

            UFOBehaviour otherUFO = hitCollider.GetComponent<UFOBehaviour>();
            if (otherUFO != null && !otherUFO.isDead)
            {
                float distanceToNeighbor = Vector3.Distance(transform.position, hitCollider.transform.position);

                if (distanceToNeighbor < activeSeparationRadius && distanceToNeighbor > 0.001f)
                {
                    Vector3 separationDirection = (transform.position - hitCollider.transform.position).normalized;
                    separationSum += separationDirection / distanceToNeighbor;
                }

                if (distanceToNeighbor < flockDetectionRadius)
                {
                    alignmentSum += hitCollider.transform.forward;
                    cohesionSum += hitCollider.transform.position;
                    alignmentCohesionNeighborsCount++;
                }
            }
        }

        Vector3 finalSeparation = separationSum * activeSeparationWeight;
        Vector3 finalAlignment = (alignmentCohesionNeighborsCount > 0 ? (alignmentSum / alignmentCohesionNeighborsCount).normalized : Vector3.zero) * activeAlignmentWeight;
        Vector3 finalCohesion = (alignmentCohesionNeighborsCount > 0 ? ((cohesionSum / alignmentCohesionNeighborsCount) - transform.position).normalized : Vector3.zero) * activeCohesionWeight;

        return finalSeparation + finalAlignment + finalCohesion;
    }

    private void MaintainAltitude()
    {
        if (playerTransform == null) return;

        float targetY = playerTransform.position.y + instanceRelativeAltitude;

        float altitudeError = targetY - transform.position.y;

        float desiredVerticalVelocity = altitudeError * altitudeCorrectionForce;

        float verticalVelocityLimit = instanceMovementSpeed * 1.5f;
        desiredVerticalVelocity = Mathf.Clamp(desiredVerticalVelocity, -verticalVelocityLimit, verticalVelocityLimit);

        float newYVelocity = Mathf.Lerp(rb.velocity.y, desiredVerticalVelocity, Time.fixedDeltaTime * altitudeResponsiveness);

        rb.velocity = new Vector3(rb.velocity.x, newYVelocity, rb.velocity.z);
    }

    public IEnumerator AttackCoroutine()
    {
        if (isDead || laserWeapon == null || playerTransform == null || mainPlayerTarget == null)
        {
            currentState = AIState.CirclingPlayer;
            attackProcessCoroutine = null;
            yield break;
        }

        rb.velocity *= 0.5f;

        Vector3 predictedTargetPos = playerTransform.position;
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            float distance = Vector3.Distance(firePoint.position, playerTransform.position);
            float laserSpeed = laserWeapon != null ? laserWeapon.laserSpeed : 100f;
            float laserTravelTime = distance / laserSpeed;
            predictedTargetPos += playerRb.velocity * laserTravelTime * aimAheadFactor;
        }

        Quaternion initialRotation = transform.rotation;
        Quaternion targetLookRotation = Quaternion.LookRotation(predictedTargetPos - firePoint.position);

        float timeElapsed = 0f;
        while (timeElapsed < aimingTime)
        {
            if (isDead || playerTransform == null)
            {
                currentState = AIState.CirclingPlayer;
                attackProcessCoroutine = null;
                yield break;
            }
            predictedTargetPos = playerTransform.position;
            if (playerRb != null)
            {
                float distance = Vector3.Distance(firePoint.position, playerTransform.position);
                float laserSpeed = laserWeapon != null ? laserWeapon.laserSpeed : 100f;
                float laserTravelTime = distance / laserSpeed;
                predictedTargetPos += playerRb.velocity * laserTravelTime * aimAheadFactor;
            }
            targetLookRotation = Quaternion.LookRotation(predictedTargetPos - firePoint.position);

            transform.rotation = Quaternion.Slerp(initialRotation, targetLookRotation, timeElapsed / aimingTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetLookRotation;

        if (laserWeapon != null)
        {
            laserWeapon.FireLaser(firePoint.position, predictedTargetPos, mainPlayerTarget);
            lastAttackTime = Time.time;
        }


        currentState = AIState.CirclingPlayer;
        attackProcessCoroutine = null;
    }

    public void HitPlayer()
    {
        Debug.Log("UFO_AI: HitPlayer called (damage should be applied by LaserWeapon or specific collision).");
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. Current Health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        currentState = AIState.Dead;
        Debug.Log(gameObject.name + " has died.");

        StopAllCoroutines();
        attackProcessCoroutine = null;

        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        if (laserWeapon != null)
        {
            laserWeapon.DestroyAllLasers();
        }

        if (mainPlayerTarget != null && mainPlayerTarget.ScoreManager != null)
        {
            mainPlayerTarget.ScoreManager.AddScore(deathScoreValue);
        }
        else
        {
            Debug.LogWarning("Cannot add score for " + gameObject.name + ": MainPlayer or MainPlayer.ScoreManager is null.");
        }

        EnemyDied?.Invoke(this);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
        {
            if (rend != meshRenderer) rend.enabled = false;
        }
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            if (ps != explosionEffect) ps.Stop();
        }


        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Destroy(gameObject, destructionDelay);
    }
}