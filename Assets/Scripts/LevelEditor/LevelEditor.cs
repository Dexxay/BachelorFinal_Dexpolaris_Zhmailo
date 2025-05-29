using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditor : MonoBehaviour
{
    [Header("Object Prefabs")]
    [SerializeField] private GameObject startAsteroidPrefab;
    [SerializeField] private List<GameObject> randomAsteroidPrefabs = new List<GameObject>();
    [SerializeField] private GameObject plasmaGunAsteroidPrefab;
    [SerializeField] private GameObject laserSMGAsteroidPrefab;
    [SerializeField] private GameObject ammo1AsteroidPrefab;
    [SerializeField] private GameObject ammo2AsteroidPrefab;
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private GameObject ufoSpawnerPrefab;
    [SerializeField] private GameObject finishAsteroidPrefab;

    [Header("Editor Settings")]
    [SerializeField] private GameObject levelObjectsParent;
    [SerializeField] private float gridSpacing = 1f;
    [SerializeField] private float randomHeightRange = 0.5f;
    [SerializeField] private Collider editorPlaneCollider;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Time Limit Settings")]
    [SerializeField] private float minSliderTimeLimit = 60f;
    [SerializeField] private float maxSliderTimeLimit = 180f;

    [HideInInspector] public GameObject startAsteroidInstance = null;
    [HideInInspector] public GameObject finishAsteroidInstance = null;

    private EditorCameraMovement cameraMovement;
    private EditorObjectPlacement objectPlacement;
    [SerializeField] private EditorUIHandler uiHandler;
    private LevelSaveLoadManager saveLoadManager;

    private string saveSlot1 = "level_slot_1.dat";
    private string saveSlot2 = "level_slot_2.dat";
    private string saveSlot3 = "level_slot_3.dat";

    private float currentLevelTimeLimit = 60f;

    public float GridSpacing { get { return gridSpacing; } }
    public float RandomHeightRange { get { return randomHeightRange; } }
    public Collider EditorPlaneCollider { get { return editorPlaneCollider; } }
    public EditorUIHandler UiHandler { get { return uiHandler; } }
    public GameObject LevelObjectsParent { get { return levelObjectsParent; } }
    public float MinSliderTimeLimit { get { return minSliderTimeLimit; } }
    public float MaxSliderTimeLimit { get { return maxSliderTimeLimit; } }


    void Awake()
    {
        cameraMovement = GetComponent<EditorCameraMovement>();
        objectPlacement = GetComponent<EditorObjectPlacement>();
        saveLoadManager = GetComponent<LevelSaveLoadManager>();

        if (cameraMovement == null) Debug.LogError("EditorCameraMovement not found!");
        if (objectPlacement == null) Debug.LogError("EditorObjectPlacement not found!");
        if (uiHandler == null) Debug.LogError("EditorUIHandler not assigned in the Inspector!");
        if (saveLoadManager == null) Debug.LogError("LevelSaveLoadManager not found!");

        if (uiHandler != null) uiHandler.Init(this);
        if (cameraMovement != null && editorPlaneCollider != null && Camera.main != null) cameraMovement.Init(Camera.main, editorPlaneCollider);
        else if (cameraMovement != null && editorPlaneCollider == null) Debug.LogError("EditorPlaneCollider is not assigned for EditorCameraMovement in the Inspector!");

        if (objectPlacement != null && Camera.main != null) objectPlacement.Init(this, Camera.main);


        if (levelObjectsParent == null)
        {
            Debug.LogWarning("levelObjectsParent was not assigned in the Inspector. Creating a new one.");
            levelObjectsParent = new GameObject("LevelObjectsParent");
        }
    }

    void Update()
    {
        if (uiHandler == null) return;

        bool isConfirmationDialogOpen = uiHandler.IsConfirmationDialogOpen();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isConfirmationDialogOpen)
            {
                uiHandler.TogglePauseMenu();
            }
        }

        if (uiHandler.IsPauseMenuOpen() || isConfirmationDialogOpen)
        {
            return;
        }

        if (cameraMovement != null) cameraMovement.HandleInput();
    }

    public float GetTimeLimit()
    {
        return currentLevelTimeLimit;
    }

    public void SetTimeLimit(float newTimeLimit)
    {
        currentLevelTimeLimit = Mathf.Clamp(newTimeLimit, minSliderTimeLimit, maxSliderTimeLimit);
        if (uiHandler != null)
        {
            uiHandler.UpdateTimeLimitText(currentLevelTimeLimit);
        }
    }

    public void ShowMessage(string message, bool isError)
    {
        if (uiHandler == null)
        {
            Debug.LogError("uiHandler is not assigned. Cannot show message.");
            return;
        }

        if (isError)
        {
            uiHandler.SetRedColor();
        }
        else
        {
            uiHandler.SetGreenColor();
        }
        uiHandler.ShowError(message);
    }

    public void ClearMessage()
    {
        if (uiHandler != null)
        {
            uiHandler.ClearMessage();
        }
    }

    public void GoToMainMenuConfirmed()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("mainMenuSceneName is not set in the Inspector. Cannot load Main Menu. Loading scene with index 0 as a fallback.");
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void SaveLevelConfirmed(int slot)
    {
        if (saveLoadManager == null)
        {
            ShowMessage("Error: SaveLoadManager is not configured.", true);
            return;
        }
        if (levelObjectsParent == null)
        {
            ShowMessage("Error: levelObjectsParent is not configured.", true);
            return;
        }


        if (startAsteroidInstance == null)
        {
            ShowMessage("Error: Start asteroid is not placed.", true);
            return;
        }
        if (finishAsteroidInstance == null)
        {
            ShowMessage("Error: Finish asteroid is not placed.", true);
            return;
        }

        string filePath = GetSavePathBySlot(slot);
        if (string.IsNullOrEmpty(filePath))
        {
            ShowMessage($"Error: Could not determine save path for slot {slot}.", true);
            return;
        }
        saveLoadManager.SaveLevel(filePath, levelObjectsParent, currentLevelTimeLimit, startAsteroidInstance, finishAsteroidInstance);
        ShowMessage($"Level saved to slot {slot}.", false);
    }

    public void LoadLevelConfirmed(int slot)
    {
        if (saveLoadManager == null)
        {
            ShowMessage("Error: SaveLoadManager is not configured.", true);
            return;
        }
        if (levelObjectsParent == null)
        {
            ShowMessage("Error: levelObjectsParent is not configured.", true);
            return;
        }

        ClearAllObjectsConfirmedInternal();
        string filePath = GetSavePathBySlot(slot);
        if (string.IsNullOrEmpty(filePath))
        {
            ShowMessage($"Error: Could not determine load path for slot {slot}.", true);
            return;
        }

        LevelData loadedData = saveLoadManager.LoadLevelForEditor(filePath, levelObjectsParent, this);
        if (loadedData != null)
        {
            currentLevelTimeLimit = loadedData.timeLimit;
            if (uiHandler != null) uiHandler.UpdateTimeLimitText(currentLevelTimeLimit);

            startAsteroidInstance = null;
            finishAsteroidInstance = null;

            foreach (Transform child in levelObjectsParent.transform)
            {
                PlacedObjectInfo objInfo = child.GetComponent<PlacedObjectInfo>();
                if (objInfo != null && !string.IsNullOrEmpty(objInfo.originalPrefabName))
                {
                    if (startAsteroidPrefab != null && objInfo.originalPrefabName == startAsteroidPrefab.name)
                    {
                        startAsteroidInstance = child.gameObject;
                    }
                    if (finishAsteroidPrefab != null && objInfo.originalPrefabName == finishAsteroidPrefab.name)
                    {
                        finishAsteroidInstance = child.gameObject;
                    }
                }
            }
            ShowMessage($"Level loaded from slot {slot}.", false);
        }
        else
        {
            ShowMessage($"Error loading level from slot {slot}. File might not exist or is corrupted.", true);
        }
    }

    public void DeleteLevelConfirmed(int slot)
    {
        if (saveLoadManager == null)
        {
            ShowMessage("Error: SaveLoadManager is not configured.", true);
            return;
        }
        string filePath = GetSavePathBySlot(slot);
        if (string.IsNullOrEmpty(filePath))
        {
            ShowMessage($"Error: Could not determine path for slot {slot} to delete.", true);
            return;
        }

        if (saveLoadManager.DeleteLevel(filePath))
        {
            ShowMessage($"Level in slot {slot} deleted.", false);
        }
        else
        {
            ShowMessage($"Failed to delete level in slot {slot}. File may not exist.", true);
        }
    }

    private void ClearAllObjectsConfirmedInternal()
    {
        if (levelObjectsParent == null)
        {
            Debug.LogWarning("levelObjectsParent is null. Cannot clear objects.");
            return;
        }

        foreach (Transform child in levelObjectsParent.transform)
        {
            Destroy(child.gameObject);
        }
        startAsteroidInstance = null;
        finishAsteroidInstance = null;
        ShowMessage("All objects cleared.", false);
    }

    public void ClearAllObjectsConfirmed()
    {
        ClearAllObjectsConfirmedInternal();
    }

    public GameObject GetPrefabByType(int objectType)
    {
        switch (objectType)
        {
            case 1: return startAsteroidPrefab;
            case 2:
                return randomAsteroidPrefabs != null && randomAsteroidPrefabs.Count > 0 ? randomAsteroidPrefabs[0] : null;
            case 3: return plasmaGunAsteroidPrefab;
            case 4: return laserSMGAsteroidPrefab;
            case 5: return ammo1AsteroidPrefab;
            case 6: return ammo2AsteroidPrefab;
            case 7: return healthPackPrefab;
            case 8: return turretPrefab;
            case 9: return ufoSpawnerPrefab;
            case 0: return finishAsteroidPrefab;
            default:
                Debug.LogWarning($"Unknown object type: {objectType}");
                return null;
        }
    }

    public GameObject GetRandomAsteroidPrefab()
    {
        if (randomAsteroidPrefabs != null && randomAsteroidPrefabs.Count > 0)
        {
            return randomAsteroidPrefabs[UnityEngine.Random.Range(0, randomAsteroidPrefabs.Count)];
        }
        else
        {
            Debug.LogWarning("Random asteroid prefabs list is empty or null.");
            return null;
        }
    }

    public bool CheckIfCanPlace(Vector3 placementPosition, GameObject prefabToSpawn, GameObject objectToIgnore = null)
    {
        if (LevelObjectsParent == null)
        {
            Debug.LogError("LevelObjectsParent in LevelEditor is not assigned during placement check.");
            return false;
        }

        ObjectRadius newObjectRadiusComponent = prefabToSpawn.GetComponent<ObjectRadius>();
        float newObjectRadius = newObjectRadiusComponent != null ? newObjectRadiusComponent.Radius : 1f;

        if (prefabToSpawn == startAsteroidPrefab && startAsteroidInstance != null && startAsteroidInstance != objectToIgnore)
        {
            ShowMessage("There is a start asteroid on a map already. Delete old one to place a new one", true);
            return false;
        }
        if (prefabToSpawn == finishAsteroidPrefab && finishAsteroidInstance != null && finishAsteroidInstance != objectToIgnore)
        {
            ShowMessage("There is a finish asteroid on a map already. Delete old one to place a new one", true);
            return false;
        }

        bool canPlace = true;

        foreach (Transform child in LevelObjectsParent.transform)
        {
            if (child.gameObject == objectToIgnore) continue;

            ObjectRadius existingObjectRadiusComponent = child.GetComponent<ObjectRadius>();
            if (existingObjectRadiusComponent != null)
            {
                float distanceXZ = Vector2.Distance(new Vector2(placementPosition.x, placementPosition.z), new Vector2(child.position.x, child.position.z));

                if (distanceXZ < newObjectRadius + existingObjectRadiusComponent.Radius)
                {
                    canPlace = false;
                    break;
                }
            }
        }

        return canPlace;
    }


    public GameObject GetPrefabByName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;

        if (startAsteroidPrefab != null && startAsteroidPrefab.name == prefabName) return startAsteroidPrefab;
        if (finishAsteroidPrefab != null && finishAsteroidPrefab.name == prefabName) return finishAsteroidPrefab;
        if (randomAsteroidPrefabs != null)
        {
            foreach (var prefab in randomAsteroidPrefabs)
            {
                if (prefab != null && prefab.name == prefabName) return prefab;
            }
        }
        if (plasmaGunAsteroidPrefab != null && plasmaGunAsteroidPrefab.name == prefabName) return plasmaGunAsteroidPrefab;
        if (laserSMGAsteroidPrefab != null && laserSMGAsteroidPrefab.name == prefabName) return laserSMGAsteroidPrefab;
        if (ammo1AsteroidPrefab != null && ammo1AsteroidPrefab.name == prefabName) return ammo1AsteroidPrefab;
        if (ammo2AsteroidPrefab != null && ammo2AsteroidPrefab.name == prefabName) return ammo2AsteroidPrefab;
        if (healthPackPrefab != null && healthPackPrefab.name == prefabName) return healthPackPrefab;
        if (turretPrefab != null && turretPrefab.name == prefabName) return turretPrefab;
        if (ufoSpawnerPrefab != null && ufoSpawnerPrefab.name == prefabName) return ufoSpawnerPrefab;

        Debug.LogWarning($"Prefab with name '{prefabName}' not found in LevelEditor's list.");
        return null;
    }

    private string GetSavePathBySlot(int slot)
    {
        string fileName;
        switch (slot)
        {
            case 1: fileName = saveSlot1; break;
            case 2: fileName = saveSlot2; break;
            case 3: fileName = saveSlot3; break;
            default:
                Debug.LogError($"Invalid slot number: {slot}");
                return null;
        }
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}