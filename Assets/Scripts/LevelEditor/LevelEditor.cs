using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class LevelEditor : MonoBehaviour
{
    [Header("Object Prefabs")]
    public GameObject startAsteroidPrefab;
    public List<GameObject> randomAsteroidPrefabs;
    public GameObject plasmaGunAsteroidPrefab;
    public GameObject laserSMGAsteroidPrefab;
    public GameObject ammo1AsteroidPrefab;
    public GameObject ammo2AsteroidPrefab;
    public GameObject healthPackPrefab;
    public GameObject turretPrefab;
    public GameObject ufoSpawnerPrefab;
    public GameObject finishAsteroidPrefab;

    [Header("Editor Settings")]
    public GameObject levelObjectsParent;
    public float gridSpacing = 1f;
    public float randomHeightRange = 0.5f;
    public Collider editorPlaneCollider;

    [Header("Time Limit Settings")]
    public float minSliderTimeLimit = 60f;
    public float maxSliderTimeLimit = 180f;

    [HideInInspector] public GameObject startAsteroidInstance = null;
    [HideInInspector] public GameObject finishAsteroidInstance = null;

    private EditorCameraMovement cameraMovement;
    private EditorObjectPlacement objectPlacement;
    public EditorUIHandler uiHandler;
    private LevelSaveLoadManager saveLoadManager;

    private string saveSlot1 = "level_slot_1.dat";
    private string saveSlot2 = "level_slot_2.dat";
    private string saveSlot3 = "level_slot_3.dat";

    private float currentLevelTimeLimit = 60f;

    void Awake()
    {
        cameraMovement = GetComponent<EditorCameraMovement>();
        objectPlacement = GetComponent<EditorObjectPlacement>();
        uiHandler = GetComponent<EditorUIHandler>();
        saveLoadManager = GetComponent<LevelSaveLoadManager>();

        if (cameraMovement == null) Debug.LogError("EditorCameraMovement not found!");
        if (objectPlacement == null) Debug.LogError("EditorObjectPlacement not found!");
        if (uiHandler == null) Debug.LogError("EditorUIHandler not found!");
        if (saveLoadManager == null) Debug.LogError("LevelSaveLoadManager not found!");

        if (uiHandler != null) uiHandler.Init(this);
        if (cameraMovement != null && editorPlaneCollider != null && Camera.main != null) cameraMovement.Init(Camera.main, editorPlaneCollider);
        if (objectPlacement != null && Camera.main != null) objectPlacement.Init(this, Camera.main);

        if (levelObjectsParent == null)
        {
            levelObjectsParent = new GameObject("LevelObjectsParent");
        }
    }

    void Update()
    {
        bool isConfirmationDialogOpen = uiHandler != null && uiHandler.IsConfirmationDialogOpen();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiHandler != null && !isConfirmationDialogOpen)
            {
                uiHandler.TogglePauseMenu();
            }
        }

        if (uiHandler != null && uiHandler.IsPauseMenuOpen() || isConfirmationDialogOpen)
        {
            return;
        }


        if (cameraMovement != null) cameraMovement.HandleInput();
        if (objectPlacement != null) objectPlacement.HandleInput();
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
        if (uiHandler == null) return;

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
        SceneManager.LoadScene(0);
    }

    public void SaveLevelConfirmed(int slot)
    {
        if (saveLoadManager == null || levelObjectsParent == null)
        {
            ShowMessage("Error: Save components are not configured.", true);
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
        saveLoadManager.SaveLevel(filePath, levelObjectsParent, currentLevelTimeLimit, startAsteroidInstance, finishAsteroidInstance);
        ShowMessage($"Level saved to slot {slot}.", false);
    }

    public void LoadLevelConfirmed(int slot)
    {
        if (saveLoadManager == null || levelObjectsParent == null)
        {
            ShowMessage("Error: Load components are not configured.", true);
            return;
        }
        ClearAllObjectsConfirmedInternal();
        string filePath = GetSavePathBySlot(slot);
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
                if (objInfo != null && objInfo.originalPrefabName != null)
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
            ShowMessage($"Error loading level from slot {slot}.", true);
        }
    }

    public void DeleteLevelConfirmed(int slot)
    {
        if (saveLoadManager == null)
        {
            ShowMessage("Error: Save/Load component is not configured.", true);
            return;
        }
        string filePath = GetSavePathBySlot(slot);
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
        if (levelObjectsParent == null) return;

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
            case 2: return randomAsteroidPrefabs != null && randomAsteroidPrefabs.Count > 0 ? randomAsteroidPrefabs[UnityEngine.Random.Range(0, randomAsteroidPrefabs.Count)] : null;
            case 3: return plasmaGunAsteroidPrefab;
            case 4: return laserSMGAsteroidPrefab;
            case 5: return ammo1AsteroidPrefab;
            case 6: return ammo2AsteroidPrefab;
            case 7: return healthPackPrefab;
            case 8: return turretPrefab;
            case 9: return ufoSpawnerPrefab;
            case 0: return finishAsteroidPrefab;
            default:
                return null;
        }
    }

    public GameObject GetPrefabByName(string prefabName)
    {
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

        return null;
    }

    private string GetSavePathBySlot(int slot)
    {
        switch (slot)
        {
            case 1: return Path.Combine(Application.persistentDataPath, saveSlot1);
            case 2: return Path.Combine(Application.persistentDataPath, saveSlot2);
            case 3: return Path.Combine(Application.persistentDataPath, saveSlot3);
            default:
                return null;
        }
    }
}