using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameLevelLoader : MonoBehaviour
{
    public LevelSaveLoadManager levelSaveLoadManager;
    public List<GameObject> availablePrefabs;
    public Transform levelObjectsParentTransform;

    private MainPlayer player;
    private float timeLimit;
    private ParticleSystem rocketEffect;
    private bool rocketEffectPlayed = false;
    public float timeBeforeEndForEffect = 30f;

    void Start()
    {
        PerformLoad();
    }

    private void PerformLoad()
    {
        InitializePlayer();
        if (!ValidateDependencies())
        {
            Debug.LogError("GameLevelLoader dependencies are not met. Loading aborted.");
            return;
        }

        ClearExistingGameObjects();

        LevelData loadedLevelData = null;
        string levelType = PlayerPrefs.GetString("CurrentLevelType");

        if (levelType == "Campaign")
        {
            string levelName = PlayerPrefs.GetString("CurrentLevelName");
            loadedLevelData = LoadCampaignLevelData(levelName);
        }
        else if (levelType == "Custom")
        {
            int slot = PlayerPrefs.GetInt("CurrentLevelSlot");
            loadedLevelData = levelSaveLoadManager.LoadLevelForGame(slot, levelObjectsParentTransform, availablePrefabs);
        }
        else
        {
            Debug.LogError("Unknown level type or level not specified.");
            return;
        }


        if (loadedLevelData != null)
        {
            HandleSuccessfulLoad(loadedLevelData);
        }
        else
        {
            HandleFailedLoad();
        }
    }

    private LevelData LoadCampaignLevelData(string levelName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, levelName + ".dat");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Campaign level file not found at {filePath}");
            return null;
        }

        try
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            LevelData loadedData;
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                loadedData = (LevelData)formatter.Deserialize(stream);
            }

            if (levelObjectsParentTransform == null)
            {
                Debug.LogError("Parent transform for loaded objects not specified!");
            }
            if (availablePrefabs == null || availablePrefabs.Count == 0)
            {
                Debug.LogError("Available prefabs list is empty!");
                return loadedData;
            }

            foreach (ObjectData objectData in loadedData.objects)
            {
                GameObject prefabToInstantiate = FindPrefabByNameInList(objectData.prefabName, availablePrefabs);
                if (prefabToInstantiate != null)
                {
                    GameObject instantiatedObject = Instantiate(prefabToInstantiate, (Vector3)objectData.position, (Quaternion)objectData.rotation, levelObjectsParentTransform);

                    if (objectData.isStartAsteroid)
                    {
                        instantiatedObject.tag = "StartAsteroid";
                    }
                    if (objectData.isFinishAsteroid)
                    {
                        instantiatedObject.tag = "FinishAsteroid";
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab with name '{objectData.prefabName}' not found in available prefabs for game! Object will not be created.");
                }
            }
            Debug.Log($"Campaign level '{levelName}' loaded successfully.");
            return loadedData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading campaign level from {filePath}: {e.Message}");
            return null;
        }
    }

    private GameObject FindPrefabByNameInList(string prefabName, List<GameObject> prefabs)
    {
        foreach (var prefab in prefabs)
        {
            if (prefab != null && prefab.name == prefabName)
            {
                return prefab;
            }
        }
        return null;
    }


    private void InitializePlayer()
    {
        player = FindFirstObjectByType<MainPlayer>();
        if (player == null)
        {
            Debug.LogError("MainPlayer object not found in the scene!");
        }
    }

    private bool ValidateDependencies()
    {
        bool isValid = true;
        if (levelSaveLoadManager == null && PlayerPrefs.GetString("CurrentLevelType") == "Custom")
        {
            Debug.LogError("LevelSaveLoadManager not assigned in GameLevelLoader! Needed for custom levels.");
            isValid = false;
        }
        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogError("Available prefabs list is empty or not assigned in GameLevelLoader! Level objects loading will not be possible.");
            isValid = false;
        }
        if (levelObjectsParentTransform == null)
        {
            Debug.LogWarning("LevelObjectsParentTransform not assigned in GameLevelLoader! Level objects will be created at the scene root.");
        }
        return isValid;
    }

    private void HandleSuccessfulLoad(LevelData loadedLevelData)
    {
        timeLimit = loadedLevelData.timeLimit;
        Debug.Log($"Level loaded successfully. Time limit: {timeLimit} sec.");

        if (player != null)
        {
            FindAndSetupAsteroids();
        }
        else
        {
            Debug.LogWarning("MainPlayer object is missing. Cannot find start/finish asteroids or position player.");
        }

        SetupBlackHole(loadedLevelData);

        SetupMapTimer(loadedLevelData);

        StartCoroutine(PlayRocketEffectBeforeEnd());
    }

    private void HandleFailedLoad()
    {
        Debug.LogError($"Failed to load level.");
    }

    private void ClearExistingGameObjects()
    {
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
        else
        {
            Debug.LogWarning("LevelObjectsParentTransform is null, cannot clear existing objects before loading.");
        }
    }

    private void FindAndSetupAsteroids()
    {
        if (levelObjectsParentTransform == null)
        {
            Debug.LogWarning("levelObjectsParentTransform not assigned. Cannot find start/finish asteroids by parent object.");
            return;
        }
        if (player == null)
        {
            Debug.LogWarning("Player object is null. Cannot position player or set up camera look.");
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

        PositionPlayerAtStart(startAsteroid);
        SetupFinishAsteroidEffectsAndCamera(finishAsteroid);
    }

    private void PositionPlayerAtStart(GameObject startAsteroid)
    {
        if (player == null) return;

        if (startAsteroid != null)
        {
            if (player.CharacterController != null) player.CharacterController.enabled = false;
            if (player.PlayerMovement != null) player.PlayerMovement.enabled = false;

            player.transform.position = startAsteroid.transform.position + new Vector3(0, 4, 0);

            if (player.CharacterController != null) player.CharacterController.enabled = true;
            if (player.PlayerMovement != null) player.PlayerMovement.enabled = true;

            Debug.Log("Player teleported to start position.");
        }
        else
        {
            Debug.LogWarning("Start asteroid not found in loaded level. Cannot position player.");
        }
    }

    private void SetupFinishAsteroidEffectsAndCamera(GameObject finishAsteroid)
    {
        if (player == null) return;

        if (finishAsteroid != null)
        {
            Transform beaconEffectTransform = finishAsteroid.transform.Find("BeaconEffect");
            if (beaconEffectTransform != null)
            {
                rocketEffect = beaconEffectTransform.GetComponent<ParticleSystem>();
                if (rocketEffect != null)
                {
                    rocketEffect.Play();
                    Debug.Log("Rocket effect played at finish asteroid.");
                }
                else
                {
                    Debug.LogWarning("BeaconEffect child object does not have a ParticleSystem component.");
                }
            }
            else
            {
                Debug.LogWarning("No 'BeaconEffect' child object found under the finish asteroid.");
            }

            if (Camera.main != null && player.MouseMovement != null)
            {
                player.MouseMovement.enabled = false;
                Vector3 direction = finishAsteroid.transform.position - Camera.main.transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                player.MouseMovement.enabled = true;
                player.MouseMovement.ForceLookRotation(lookRotation, Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f));
                Debug.Log("Camera forced to look at finish asteroid.");
            }
            else
            {
                if (Camera.main == null) Debug.LogWarning("Main Camera not found. Cannot force camera look.");
                if (player.MouseMovement == null) Debug.LogWarning("Player MouseMovement component not found. Cannot force camera look.");
            }
        }
        else
        {
            Debug.LogWarning("Finish asteroid not found in loaded level. Cannot set up finish effects or camera look.");
        }
    }

    private void SetupBlackHole(LevelData loadedLevelData)
    {
        BlackHoleMover blackHoleMover = FindFirstObjectByType<BlackHoleMover>();

        if (blackHoleMover != null)
        {
            blackHoleMover.moveDurationInSeconds = loadedLevelData.timeLimit;
            Debug.Log($"Black Hole move duration set to level time limit: {loadedLevelData.timeLimit} seconds.");
        }
        else
        {
            Debug.LogWarning("BlackHoleMover script not found in the scene. Cannot configure Black Hole.");
        }
    }

    private void SetupMapTimer(LevelData loadedLevelData)
    {
        if (player != null)
        {
            MapTimer mapTimer = player.MapTimer;

            if (mapTimer != null)
            {
                mapTimer.mapDurationInSeconds = loadedLevelData.timeLimit;
                Debug.Log($"MapTimer duration set to level time limit: {loadedLevelData.timeLimit} seconds.");
            }
            else
            {
                Debug.LogWarning("MapTimer component not found on the Player object. Cannot configure Map Timer.");
            }
        }
        else
        {
            Debug.LogWarning("Player object is null. Cannot configure Map Timer.");
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
}