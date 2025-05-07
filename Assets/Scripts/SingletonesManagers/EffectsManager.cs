using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    private static EffectsManager instance;

    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private int maxHolesAmount;
    
    private GameObject[] bulletHoles;
    private int nextBulletIndex;
    
    public static EffectsManager Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        bulletHoles = new GameObject[maxHolesAmount];

        for (int i = 0; i < maxHolesAmount; i++)
        {
            bulletHoles[i] = Instantiate(bulletHolePrefab, transform);
            bulletHoles[i].SetActive(false);
        }
    }

    public GameObject SpawnBulletHole(Vector3 position, Quaternion rotation)
    {
        GameObject currentBulletHole = bulletHoles[nextBulletIndex];
        if (currentBulletHole == null)
            currentBulletHole = Instantiate(bulletHolePrefab, transform);
        currentBulletHole.SetActive(true);
        currentBulletHole.transform.SetPositionAndRotation(position, rotation);

        nextBulletIndex++;
        if (nextBulletIndex >= maxHolesAmount)
        {
            nextBulletIndex = 0;
        }
        
        return currentBulletHole;
    }
}
