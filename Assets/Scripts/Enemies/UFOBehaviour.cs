using System;
using System.Collections;
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
    [SerializeField] private float playerLostDetectionRadiusFactor = 1.5f;

    [Header("Patrolling Settings")]
    [SerializeField] private float patrolRadius = 50f;
    [SerializeField] private float patrolPointReachedDistance = 5f;
    [SerializeField] private float patrolAltitude = 20f;

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
    [SerializeField] private float circlingToApproachDistanceFactor = 1.3f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 50f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float aimAheadFactor = 0.5f;
    [SerializeField] private float aimingTime = 0.5f;

    [Header("Health & Death")]
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private int deathScoreValue = 100;
    [SerializeField] private float destructionDelay = 2f;

    [Header("Decision Tree Settings")]
    [SerializeField] private DecisionNode rootDecisionNode;
    [SerializeField] private float decisionInterval = 0.25f;
    [SerializeField] public bool enableDebugLogs = false;

    private int currentHealth;
    private Rigidbody rb;
    private MainPlayer mainPlayerTarget;
    private Transform playerTransform;
    private float lastAttackTime;
    private bool isDead = false;
    private Coroutine attackProcessCoroutine;
    private float instanceCirclingDistance;
    private float instanceRelativeAltitude;
    private float instanceMovementSpeed;

    private Vector3 spawnPosition;
    private Vector3 currentPatrolDestination;
    private bool hasPatrolDestination;

    private ActionNode currentAction = null;
    private Coroutine currentActionCoroutine = null;

    public Transform PlayerTransform => playerTransform;
    public Coroutine AttackProcessCoroutine => attackProcessCoroutine;
    public int CurrentHealth => currentHealth;
    public float PlayerDetectionRadius => playerDetectionRadius;
    public float CirclingDistance => instanceCirclingDistance;
    public float CirclingToApproachDistanceFactor => circlingToApproachDistanceFactor;
    public float AttackRange => attackRange;
    public float PlayerLostDetectionRadiusFactor => playerLostDetectionRadiusFactor;

    public event Action<IEnemy> EnemyDied;

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        InitializeBehavior();
        StartDecisionMaking();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("UFO_AI requires a Rigidbody component on " + gameObject.name);
            enabled = false;
            return;
        }
        rb.useGravity = false;

        if (laserWeapon == null) laserWeapon = GetComponentInChildren<LaserWeapon>();
        if (laserWeapon == null) Debug.LogWarning("LaserWeapon component not found on " + gameObject.name + " or its children. UFO will not be able to attack.");

        if (firePoint == null && laserWeapon != null) firePoint = laserWeapon.transform;
        else if (firePoint == null) firePoint = transform;

        currentHealth = maxHealth;
        spawnPosition = transform.position;
    }

    private void InitializeBehavior()
    {
        mainPlayerTarget = FindFirstObjectByType<MainPlayer>();
        if (mainPlayerTarget != null)
        {
            playerTransform = mainPlayerTarget.transform;
        }

        instanceCirclingDistance = circlingDistance + UnityEngine.Random.Range(-circlingDistanceVariation, circlingDistanceVariation);
        instanceCirclingDistance = Mathf.Max(1f, instanceCirclingDistance);
        instanceRelativeAltitude = relativeAltitude + UnityEngine.Random.Range(-relativeAltitudeVariation, relativeAltitudeVariation);
        instanceRelativeAltitude = Mathf.Max(2f, instanceRelativeAltitude);
        instanceMovementSpeed = movementSpeed + UnityEngine.Random.Range(-movementSpeedVariation, movementSpeedVariation);
        instanceMovementSpeed = Mathf.Max(1f, instanceMovementSpeed);

        SetNewPatrolDestination();

        lastAttackTime = -attackCooldown;
    }

    private void StartDecisionMaking()
    {
        if (rootDecisionNode == null)
        {
            Debug.LogWarning($"Root Decision Node is NOT assigned for {gameObject.name}. AI will default to Patrol and will not use the Decision Tree.");
            PatrolAction defaultPatrolAction = ScriptableObject.CreateInstance<PatrolAction>();
            currentAction = defaultPatrolAction;
            currentActionCoroutine = StartCoroutine(currentAction.Execute(this));
        }
        else
        {
            StartCoroutine(DecisionTreeCoroutine());
        }
    }

    private IEnumerator DecisionTreeCoroutine()
    {
        if (rootDecisionNode == null)
        {
            if (enableDebugLogs) Debug.LogError($"{gameObject.name}: DecisionTreeCoroutine stopping because rootDecisionNode is null.");
            yield break;
        }

        while (true)
        {
            if (isDead)
            {
                if (enableDebugLogs) Debug.Log($"{gameObject.name}: UFO is dead, stopping DecisionTreeCoroutine.");
                yield break;
            }

            UpdatePlayerTarget();

            ActionNode nextAction = rootDecisionNode.MakeDecision(this);

            if (nextAction == null)
            {
                if (enableDebugLogs) Debug.LogWarning($"{gameObject.name}: Decision Tree returned a NULL action. Sticking to current action or stopping.");
            }
            else if (nextAction != currentAction)
            {
                TransitionToAction(nextAction);
            }

            if (currentAction is DieAction && !isDead)
            {
                Die();
                yield break;
            }

            yield return new WaitForSeconds(decisionInterval);
        }
    }

    private void UpdatePlayerTarget()
    {
        if (playerTransform == null && mainPlayerTarget == null)
        {
            mainPlayerTarget = FindFirstObjectByType<MainPlayer>();
            if (mainPlayerTarget != null)
            {
                playerTransform = mainPlayerTarget.transform;
            }
        }
    }

    private void TransitionToAction(ActionNode newAction)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"{gameObject.name} state: {(currentAction != null ? currentAction.name : "None")} -> {newAction.name}. " +
                      $"PlayerDist: {(playerTransform ? Vector3.Distance(transform.position, playerTransform.position).ToString("F1") : "N/A")}, " +
                      $"CanAttack: {CanAttack()}, Health: {currentHealth}");
        }

        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        currentAction = newAction;
        currentActionCoroutine = StartCoroutine(currentAction.Execute(this));
    }

    public void SetNewPatrolDestination()
    {
        if (isDead) return;

        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        float randomDist = UnityEngine.Random.Range(patrolRadius * 0.3f, patrolRadius);
        Vector3 offset = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward * randomDist;
        currentPatrolDestination = spawnPosition + offset;
        currentPatrolDestination.y = spawnPosition.y + patrolAltitude;
        hasPatrolDestination = true;
    }

    public bool CanAttack()
    {
        if (isDead || playerTransform == null || laserWeapon == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool isInAttackRange = distanceToPlayer <= attackRange;
        bool cooldownPassed = Time.time >= lastAttackTime + attackCooldown;
        bool playerLost = distanceToPlayer > playerDetectionRadius * playerLostDetectionRadiusFactor;

        return isInAttackRange && cooldownPassed && !playerLost;
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            HandleDeadState();
            return;
        }

        if (currentAction == null)
        {
            HandleNoActionState();
            return;
        }

        if (currentAction is AttackAction)
        {
            ApplyAttackMovementModifier();
            MaintainAltitude();
            return;
        }

        if (currentAction is DieAction) return;

        HandleGeneralMovement();
        MaintainAltitude();
    }

    private void HandleDeadState()
    {
        if (rb != null && rb.isKinematic == false)
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 5f);
    }

    private void HandleNoActionState()
    {
        if (rb != null)
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), Time.fixedDeltaTime * 5f);
    }

    private void ApplyAttackMovementModifier()
    {
        if (rb != null)
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity * 0.5f, Time.fixedDeltaTime * 2f);
    }

    private void HandleGeneralMovement()
    {
        if (rb == null) return;

        Vector3 baseMovementDirection = CalculateBaseMovementDirection();
        float currentCalculatedSpeed = CalculateCurrentSpeed();

        (float sepRadius, float sepWeight, float alignWeight, float cohWeight) = GetActiveFlockingParameters();
        Vector3 flockingVector = CalculateFlockingVector(sepRadius, sepWeight, alignWeight, cohWeight);
        flockingVector.y = 0;

        Vector3 combinedSteeringDirection = baseMovementDirection.normalized + flockingVector;
        if (baseMovementDirection.sqrMagnitude < 0.01f) combinedSteeringDirection = flockingVector;

        Vector3 lookDirection = DetermineLookDirection(combinedSteeringDirection);

        ApplyRotation(lookDirection);
        ApplyHorizontalVelocity(combinedSteeringDirection, currentCalculatedSpeed);

        if ((currentAction is PatrolAction && !hasPatrolDestination) || combinedSteeringDirection.sqrMagnitude < 1e-6)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), Time.fixedDeltaTime * 2f);
        }
    }

    private Vector3 CalculateBaseMovementDirection()
    {
        if (currentAction is CirclePlayerAction && playerTransform != null)
        {
            float effectiveCirclingDistance = instanceCirclingDistance;
            float effectiveSpeedMultiplier = circlingSpeedMultiplier;

            Vector3 playerToUfoHorizontal = transform.position - playerTransform.position;
            playerToUfoHorizontal.y = 0;

            if (playerToUfoHorizontal.sqrMagnitude < 0.1f)
            {
                playerToUfoHorizontal = transform.right;
                playerToUfoHorizontal.y = 0;
                if (playerToUfoHorizontal.sqrMagnitude < 0.1f) playerToUfoHorizontal = Vector3.right;
                playerToUfoHorizontal.Normalize();
            }
            else
            {
                playerToUfoHorizontal.Normalize();
            }

            Vector3 desiredCircleHorizontalPos = playerTransform.position + playerToUfoHorizontal * effectiveCirclingDistance;
            Vector3 radiusCorrection = (desiredCircleHorizontalPos - transform.position);
            radiusCorrection.y = 0;
            Vector3 tangentialDirection = Vector3.Cross(Vector3.up, playerToUfoHorizontal);
            return (radiusCorrection.normalized * circlingRadiusCorrectionWeight + tangentialDirection.normalized * circlingTangentialWeight);
        }
        else if (currentAction is ApproachPlayerAction && playerTransform != null)
        {
            Vector3 playerHorizontalPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            return (playerHorizontalPos - transform.position);
        }
        else if (currentAction is PatrolAction)
        {
            if (!hasPatrolDestination || Vector3.Distance(new Vector3(transform.position.x, currentPatrolDestination.y, transform.position.z), currentPatrolDestination) < patrolPointReachedDistance)
            {
                SetNewPatrolDestination();
            }
            return (currentPatrolDestination - transform.position);
        }
        return Vector3.zero;
    }

    private float CalculateCurrentSpeed()
    {
        float speed = instanceMovementSpeed;
        if (currentAction is CirclePlayerAction)
        {
            speed *= circlingSpeedMultiplier;
        }
        return speed;
    }

    private (float sepRadius, float sepWeight, float alignWeight, float cohWeight) GetActiveFlockingParameters()
    {
        float activeSeparationRadius = flockDetectionRadius;
        float activeSeparationWeight = separationWeight;
        float activeAlignmentWeight = alignmentWeight;
        float activeCohesionWeight = cohesionWeight;

        if (currentAction is CirclePlayerAction)
        {
            activeSeparationRadius = circlingSeparationRadius;
            activeSeparationWeight = circlingSeparationWeight;
        }
        return (activeSeparationRadius, activeSeparationWeight, activeAlignmentWeight, activeCohesionWeight);
    }

    private Vector3 CalculateFlockingVector(float activeSeparationRadius, float activeSeparationWeight, float activeAlignmentWeight, float activeCohesionWeight)
    {
        Vector3 separationSum = Vector3.zero;
        Vector3 alignmentSum = Vector3.zero;
        Vector3 cohesionSum = Vector3.zero;
        int separationNeighborsCount = 0;
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
                    separationSum += separationDirection / Mathf.Max(distanceToNeighbor, 0.1f);
                    separationNeighborsCount++;
                }
                if (distanceToNeighbor < flockDetectionRadius)
                {
                    alignmentSum += otherUFO.transform.forward;
                    cohesionSum += hitCollider.transform.position;
                    alignmentCohesionNeighborsCount++;
                }
            }
        }
        Vector3 finalSeparation = (separationNeighborsCount > 0 ? (separationSum / separationNeighborsCount) : Vector3.zero) * activeSeparationWeight;
        Vector3 finalAlignment = (alignmentCohesionNeighborsCount > 0 ? (alignmentSum / alignmentCohesionNeighborsCount).normalized : Vector3.zero) * activeAlignmentWeight;
        Vector3 finalCohesion = (alignmentCohesionNeighborsCount > 0 ? ((cohesionSum / alignmentCohesionNeighborsCount) - transform.position).normalized : Vector3.zero) * activeCohesionWeight;
        return finalSeparation + finalAlignment + finalCohesion;
    }

    private Vector3 DetermineLookDirection(Vector3 combinedSteeringDirection)
    {
        Vector3 lookDirection = combinedSteeringDirection;
        if ((currentAction is ApproachPlayerAction || currentAction is CirclePlayerAction) && playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.01f) lookDirection = directionToPlayer;
        }
        else if (currentAction is PatrolAction)
        {
            if (combinedSteeringDirection.sqrMagnitude < 0.01f && hasPatrolDestination)
            {
                lookDirection = (currentPatrolDestination - transform.position);
                lookDirection.y = 0;
            }
        }
        return lookDirection;
    }

    private void ApplyRotation(Vector3 lookDirection)
    {
        if (lookDirection.sqrMagnitude > 1e-6)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void ApplyHorizontalVelocity(Vector3 combinedSteeringDirection, float currentCalculatedSpeed)
    {
        Vector3 desiredHorizontalVelocity = Vector3.zero;
        if (combinedSteeringDirection.sqrMagnitude > 1e-6)
        {
            desiredHorizontalVelocity = combinedSteeringDirection.normalized * currentCalculatedSpeed;
        }

        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, desiredHorizontalVelocity, Time.fixedDeltaTime * horizontalMovementResponsiveness);
        rb.velocity = new Vector3(newHorizontalVelocity.x, rb.velocity.y, newHorizontalVelocity.z);
    }

    private void MaintainAltitude()
    {
        if (rb == null) return;

        float targetY;
        if (currentAction is PatrolAction || playerTransform == null)
        {
            targetY = spawnPosition.y + patrolAltitude;
        }
        else
        {
            targetY = playerTransform.position.y + instanceRelativeAltitude;
        }
        float altitudeError = targetY - transform.position.y;
        float desiredVerticalVelocity = altitudeError * altitudeCorrectionForce;
        float verticalVelocityLimit = instanceMovementSpeed * 0.75f;
        desiredVerticalVelocity = Mathf.Clamp(desiredVerticalVelocity, -verticalVelocityLimit, verticalVelocityLimit);
        float newYVelocity = Mathf.Lerp(rb.velocity.y, desiredVerticalVelocity, Time.fixedDeltaTime * altitudeResponsiveness);
        rb.velocity = new Vector3(rb.velocity.x, newYVelocity, rb.velocity.z);
    }

    public void StartAttackCoroutine()
    {
        if (isDead || playerTransform == null || laserWeapon == null || mainPlayerTarget == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"{gameObject.name}: Cannot start attack coroutine due to missing dependencies or being dead.");
            attackProcessCoroutine = null;
            return;
        }

        if (attackProcessCoroutine != null)
        {
            StopCoroutine(attackProcessCoroutine);
        }
        attackProcessCoroutine = StartCoroutine(AttackCoroutine());
    }

    public IEnumerator AttackCoroutine()
    {
        if (isDead || laserWeapon == null || playerTransform == null || mainPlayerTarget == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"{gameObject.name}: AttackCoroutine: Prerequisites not met, ending coroutine.");
            attackProcessCoroutine = null;
            yield break;
        }

        float timeElapsed = 0f;

        while (timeElapsed < aimingTime)
        {
            if (isDead || playerTransform == null)
            {
                if (enableDebugLogs) Debug.Log($"{gameObject.name}: AttackCoroutine: Player lost or UFO died during aiming.");
                attackProcessCoroutine = null;
                yield break;
            }

            Vector3 predictedTargetPos = PredictTargetPosition(playerTransform, mainPlayerTarget.GetComponent<Rigidbody>(), aimAheadFactor);
            AimAtPosition(predictedTargetPos);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPredictedTargetPos = PredictTargetPosition(playerTransform, mainPlayerTarget.GetComponent<Rigidbody>(), aimAheadFactor);
        AimAtPosition(finalPredictedTargetPos);

        FireWeapon(finalPredictedTargetPos);
    }

    private Vector3 PredictTargetPosition(Transform targetTransform, Rigidbody targetRb, float factor)
    {
        Vector3 predictedTargetPos = targetTransform.position;
        if (targetRb != null)
        {
            float distance = Vector3.Distance(firePoint.position, targetTransform.position);
            float laserSpeedVal = laserWeapon != null ? laserWeapon.laserSpeed : 100f;
            if (laserSpeedVal <= 0) laserSpeedVal = 100f;
            float laserTravelTime = distance / laserSpeedVal;
            predictedTargetPos += targetRb.velocity * laserTravelTime * factor;
        }
        predictedTargetPos.y += 0.5f;
        return predictedTargetPos;
    }

    private void AimAtPosition(Vector3 targetPosition)
    {
        if ((targetPosition - firePoint.position).sqrMagnitude > 0.01f)
        {
            Quaternion targetLookRotation = Quaternion.LookRotation((targetPosition - firePoint.position).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetLookRotation, Time.deltaTime * rotationSpeed * 2f);
        }
    }

    private void FireWeapon(Vector3 targetPosition)
    {
        if (laserWeapon != null)
        {
            laserWeapon.FireLaser(firePoint.position, targetPosition, mainPlayerTarget);
            lastAttackTime = Time.time;
            attackProcessCoroutine = null;
        }
        else
        {
            if (enableDebugLogs) Debug.LogWarning($"{gameObject.name}: LaserWeapon is null, cannot fire.");
            attackProcessCoroutine = null;
        }
    }

    public void HitPlayer() { }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;
        currentHealth -= amount;
        if (enableDebugLogs) Debug.Log($"{gameObject.name} took {amount} damage. Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (enableDebugLogs) Debug.Log($"{gameObject.name} is Die() method called. Initiating destruction.");

        StopAllUfoCoroutines();
        PerformDeathEffects();
        AwardScoreAndInvokeEvent();
        DisableComponentsAndDestroy();
    }

    private void StopAllUfoCoroutines()
    {
        if (attackProcessCoroutine != null)
        {
            StopCoroutine(attackProcessCoroutine);
            attackProcessCoroutine = null;
        }
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }
        StopAllCoroutines();
    }

    private void PerformDeathEffects()
    {
        if (explosionEffect != null)
        {
            explosionEffect.Play();
        }

        if (laserWeapon != null)
        {
            laserWeapon.DestroyAllLasers();
        }
    }

    private void AwardScoreAndInvokeEvent()
    {
        if (mainPlayerTarget != null && mainPlayerTarget.ScoreManager != null)
        {
            mainPlayerTarget.ScoreManager.AddScore(deathScoreValue);
        }
        EnemyDied?.Invoke(this);
    }

    private void DisableComponentsAndDestroy()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        Destroy(gameObject, destructionDelay);
    }
}