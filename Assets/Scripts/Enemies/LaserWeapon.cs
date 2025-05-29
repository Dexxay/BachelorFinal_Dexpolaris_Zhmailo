using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] public float laserSpeed = 50f;
    [SerializeField] private int laserDamage = 25;
    [SerializeField] private float accuracy = 2.5f;
    [SerializeField] private float distanceTravelMultiplier = 2f;
    [SerializeField] private LineRenderer laserLinePrefab;
    [SerializeField] private LayerMask groundLayer;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shotSound;

    private List<GameObject> activeLasers = new();

    public void FireLaser(Vector3 startPosition, Vector3 targetPosition, MainPlayer playerTarget)
    {
        if (laserLinePrefab == null)
        {
            Debug.LogWarning("LaserLinePrefab is not assigned in LaserWeapon!");
            return;
        }
        if (playerTarget == null)
        {
            Debug.LogWarning("PlayerTarget is null in LaserWeapon.FireLaser!");
            return;
        }


        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && shotSound != null) audioSource.PlayOneShot(shotSound);

        Vector3 direction = (targetPosition - startPosition).normalized;
        float distanceToTarget = Vector3.Distance(startPosition, targetPosition);
        Vector3 extendedTargetPosition = startPosition + direction * (distanceToTarget * distanceTravelMultiplier);

        GameObject laserGO = new GameObject("LaserBeam");

        LineRenderer lr = Instantiate(laserLinePrefab, laserGO.transform);
        lr.positionCount = 2;
        lr.SetPosition(0, startPosition);
        lr.SetPosition(1, startPosition);

        activeLasers.Add(laserGO);

        StartCoroutine(MoveLaser(lr, startPosition, extendedTargetPosition, laserGO, playerTarget));
    }

    private IEnumerator MoveLaser(LineRenderer lr, Vector3 start, Vector3 target, GameObject laserGO, MainPlayer playerTarget)
    {
        float elapsed = 0f;
        float totalDistance = Vector3.Distance(start, target);
        float travelTime = totalDistance / laserSpeed;
        bool hasHitPlayer = false;

        Color startColor = lr.startColor;
        startColor.a = 1f;
        lr.startColor = startColor;
        lr.endColor = startColor;

        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;

        while (elapsed < travelTime)
        {
            if (laserGO == null || lr == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            Vector3 currentPosition = Vector3.Lerp(start, target, elapsed / travelTime);
            lr.SetPosition(1, currentPosition);

            float alpha = Mathf.Lerp(1f, 0f, elapsed / travelTime);
            Color currentColor = lr.startColor;
            currentColor.a = alpha;
            lr.startColor = currentColor;
            lr.endColor = currentColor;

            float width = Mathf.Lerp(0.2f, 0f, elapsed / travelTime);
            lr.startWidth = width;
            lr.endWidth = width;

            Vector3 rayDir = (currentPosition - start).normalized;
            float rayDist = Vector3.Distance(start, currentPosition);

            RaycastHit hit;
            if (Physics.Raycast(start, rayDir, out hit, rayDist, groundLayer))
            {
                lr.SetPosition(1, hit.point);

                Vector3 position = hit.point + hit.normal * 0.01f;
                Quaternion rotation = Quaternion.LookRotation(hit.normal);

                if (EffectsManager.Instance != null)
                {
                    GameObject bulletHole = EffectsManager.Instance.SpawnBulletHole(position, rotation);
                    if (bulletHole != null) bulletHole.transform.SetParent(hit.transform, true);
                }
                else
                {
                    Debug.LogWarning("EffectsManager.Instance is null. Cannot spawn bullet hole.");
                }

                Destroy(lr.gameObject);
                laserGO = null;
                break;
            }

            if (!hasHitPlayer && playerTarget != null && Vector3.Distance(currentPosition, playerTarget.transform.position) < accuracy)
            {
                ApplyDamageToTarget(playerTarget);
                hasHitPlayer = true;
                Destroy(lr.gameObject);
                laserGO = null;
                break;
            }

            yield return null;
        }

        if (laserGO != null)
        {
            activeLasers.Remove(laserGO);
            Destroy(laserGO);
        }
        else if (lr != null && lr.gameObject != null)
        {
            Destroy(lr.gameObject);
        }
    }


    public void ApplyDamageToTarget(MainPlayer playerTarget)
    {
        if (playerTarget != null && playerTarget.PlayerHealthManager != null)
        {
            playerTarget.PlayerHealthManager.ReduceHealth(laserDamage);
        }
        else
        {
            Debug.LogWarning("Cannot apply damage: playerTarget or PlayerHealthManager is null.");
        }
    }

    public void DestroyAllLasers()
    {
        foreach (var laser in activeLasers)
        {
            if (laser != null)
            {
                foreach (Transform child in laser.transform)
                {
                    Destroy(child.gameObject);
                }
                Destroy(laser);
            }
        }
        activeLasers.Clear();
    }

    private void OnDestroy()
    {
        DestroyAllLasers();
    }
}