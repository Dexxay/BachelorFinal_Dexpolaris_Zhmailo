using UnityEngine;
using System.Collections.Generic;
using System.IO;

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

    [HideInInspector] public GameObject startAsteroidInstance = null;
    [HideInInspector] public GameObject finishAsteroidInstance = null;

    private EditorCameraMovement cameraMovement;
    private EditorObjectPlacement objectPlacement;
    private EditorUIHandler uiHandler;
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

        if (cameraMovement == null) Debug.LogError("EditorCameraMovement component not found!");
        if (objectPlacement == null) Debug.LogError("EditorObjectPlacement component not found!");
        if (uiHandler == null) Debug.LogError("EditorUIHandler component not found!");
        if (saveLoadManager == null) Debug.LogError("LevelSaveLoadManager component not found!");

        if (levelObjectsParent == null)
        {
            Debug.LogError("Level Objects Parent not assigned!");
            enabled = false;
            return;
        }
        if (editorPlaneCollider == null)
        {
            Debug.LogError("Editor plane collider not assigned!");
            enabled = false;
            return;
        }

        cameraMovement.Init(Camera.main, editorPlaneCollider);
        objectPlacement.Init(this, Camera.main);
        uiHandler.Init(this);
        saveLoadManager.Init(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiHandler != null)
            {
                uiHandler.TogglePauseMenu();
            }
        }

        if (uiHandler != null && uiHandler.IsPauseMenuOpen())
        {
            return;
        }

        if (cameraMovement != null) cameraMovement.HandleInput();
        if (objectPlacement != null) objectPlacement.HandleInput();
    }

    public void SetTimeLimit(float time)
    {
        currentLevelTimeLimit = time;
        uiHandler.UpdateTimeLimitText(currentLevelTimeLimit);
    }

    public float GetTimeLimit()
    {
        return currentLevelTimeLimit;
    }

    public void SaveLevel(int slot)
    {
        saveLoadManager.SaveLevel(slot);
    }

    public void LoadLevel(int slot)
    {
        saveLoadManager.LoadLevel(slot);
    }

    public void ClearLevel()
    {
        if (levelObjectsParent != null)
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in levelObjectsParent.transform)
            {
                childrenToDestroy.Add(child.gameObject);
            }
            foreach (GameObject child in childrenToDestroy)
            {
                DestroyImmediate(child);
            }
        }
        startAsteroidInstance = null;
        finishAsteroidInstance = null;
        if (uiHandler != null) uiHandler.ClearMessage();
        Debug.Log("Level cleared.");
        ShowMessage("Level cleared. ", false);
    }

    public void ShowMessage(string message, bool isError)
    {
        if (isError)
            uiHandler.SetRedColor();
        else
            uiHandler.SetGreenColor();
        if (uiHandler != null) uiHandler.ShowError(message);
    }

    public void ClearMessage()
    {
        if (uiHandler != null) uiHandler.ClearMessage();
    }

    public string GetSaveFilePath(int slot)
    {
        return saveLoadManager.GetSaveFilePath(slot);
    }

    public GameObject GetPrefabToSpawn(int objectType)
    {
        switch (objectType)
        {
            case 1: return startAsteroidPrefab;
            case 2:
                if (randomAsteroidPrefabs != null && randomAsteroidPrefabs.Count > 0)
                {
                    return randomAsteroidPrefabs[Random.Range(0, randomAsteroidPrefabs.Count)];
                }
                Debug.LogWarning("Random asteroid prefabs list is empty!");
                return null;
            case 3: return plasmaGunAsteroidPrefab;
            case 4: return laserSMGAsteroidPrefab;
            case 5: return ammo1AsteroidPrefab;
            case 6: return ammo2AsteroidPrefab;
            case 7: return healthPackPrefab;
            case 8: return turretPrefab;
            case 9: return ufoSpawnerPrefab;
            case 0: return finishAsteroidPrefab;
            default:
                Debug.LogWarning($"Invalid object type requested: {objectType}");
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

        Debug.LogWarning($"Prefab with name '{prefabName}' not found in LevelEditor.");
        return null;
    }
}