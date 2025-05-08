using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLevelLoader : MonoBehaviour
{
    public LevelSaveLoadManager levelSaveLoadManager;
    public List<GameObject> availablePrefabs;
    public Transform levelObjectsParentTransform;
    public int levelSlotToLoad = 1;

    private MainPlayer player;
    private float timeLimit;
    private ParticleSystem rocketEffect;
    private bool rocketEffectPlayed = false;
    public float timeBeforeEndForEffect = 30f;

    void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();

        if (levelSaveLoadManager == null)
        {
            Debug.LogError("LevelSaveLoadManager not assigned in GameLevelLoader!");
            return;
        }
        if (levelObjectsParentTransform == null)
        {
            Debug.LogError("LevelObjectsParentTransform not assigned in GameLevelLoader! Level objects will be created at the scene root.");
        }
        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogError("Available prefabs list is empty or not assigned in GameLevelLoader! Level objects loading will not be possible.");
            return;
        }

        LevelData loadedLevelData = levelSaveLoadManager.LoadLevelForGame(levelSlotToLoad, levelObjectsParentTransform, availablePrefabs);

        if (loadedLevelData != null)
        {
            timeLimit = loadedLevelData.timeLimit;
            Debug.Log($"Level loaded successfully. Time limit: {timeLimit} sec.");
            FindStartAndFinishAsteroids(loadedLevelData);
            StartCoroutine(PlayRocketEffectBeforeEnd());
        }
        else
        {
            Debug.LogError("Failed to load level for game.");
        }
    }

    void FindStartAndFinishAsteroids(LevelData loadedLevelData)
    {
        if (levelObjectsParentTransform == null)
        {
            Debug.LogWarning("levelObjectsParentTransform not assigned. Cannot find start/finish asteroids by parent object.");
            return;
        }

        GameObject startAsteroid = null;
        GameObject finishAsteroid = null;

        foreach (Transform child in levelObjectsParentTransform)
        {
            if (child.CompareTag("StartAsteroid"))
            {
                startAsteroid = child.gameObject;
            }
            if (child.CompareTag("FinishAsteroid"))
            {
                finishAsteroid = child.gameObject;
            }
        }

        if (startAsteroid != null)
        {
            player.CharacterController.enabled = false;
            player.PlayerMovement.enabled = false;
            player.transform.position = startAsteroid.transform.position + new Vector3(0, 4, 0);
            player.CharacterController.enabled = true;
            player.PlayerMovement.enabled = true;
            Debug.Log("Player teleported to " + startAsteroid.transform.position);
        }
        else
        {
            Debug.LogWarning("Start asteroid not found in loaded level for game (looking for tag 'StartAsteroid').");
        }

        if (finishAsteroid != null)
        {
            Transform beaconEffectTransform = finishAsteroid.transform.Find("BeaconEffect");
            if (beaconEffectTransform != null)
            {
                rocketEffect = beaconEffectTransform.GetComponent<ParticleSystem>();
                if (rocketEffect != null)
                {
                    rocketEffect.Play();
                    Debug.Log("Rocket effect played at start.");
                }
                else
                {
                    Debug.LogWarning("BeaconEffect doesn't have ParticleSystem.");
                }
            }
            else
            {
                Debug.LogWarning("No BeaconEffect object found");
            }
        }
        else
        {
            Debug.LogWarning("Finish asteroid not found in loaded level for game (looking for tag 'FinishAsteroid').");
        }

        if (Camera.main != null && finishAsteroid != null)
        {
            player.MouseMovement.enabled = false;
            Vector3 direction = finishAsteroid.transform.position - Camera.main.transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            player.MouseMovement.enabled = true;
            player.MouseMovement.ForceLookRotation(lookRotation, Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f));

        }
    }

    IEnumerator PlayRocketEffectBeforeEnd()
    {
        float delay = Mathf.Max(0, timeLimit - timeBeforeEndForEffect);
        yield return new WaitForSeconds(delay);

        if (!rocketEffectPlayed && rocketEffect != null)
        {
            rocketEffect.Play();
            rocketEffectPlayed = true;
            Debug.Log($"Rocket effect played '{timeBeforeEndForEffect}' seconds before time runs out.");
        }
    }

    public void LoadSelectedLevel(int slot)
    {
        levelSlotToLoad = slot;
        if (levelObjectsParentTransform != null)
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in levelObjectsParentTransform)
            {
                childrenToDestroy.Add(child.gameObject);
            }
            foreach (GameObject child in childrenToDestroy)
            {
                Destroy(child);
            }
        }
        Start();
    }
}


