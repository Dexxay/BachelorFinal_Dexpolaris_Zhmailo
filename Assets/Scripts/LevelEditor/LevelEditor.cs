using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelEditor : MonoBehaviour
{
    [Header("������� ��'����")]
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

    [Header("������������ ���������")]
    public GameObject levelObjectsParent;
    public float gridSpacing = 1f;
    public float randomHeightRange = 0.5f;
    public Collider editorPlaneCollider; // Collider ������� ���������

    // ��������� �� ���������� ����������� ��'����
    [HideInInspector] public GameObject startAsteroidInstance = null;
    [HideInInspector] public GameObject finishAsteroidInstance = null;

    // ���������
    private EditorCameraMovement cameraMovement;
    private EditorObjectPlacement objectPlacement;
    private EditorUIHandler uiHandler;
    private LevelSaveLoadManager saveLoadManager;

    // ������������ ����������
    private string saveSlot1 = "level_slot_1.dat";
    private string saveSlot2 = "level_slot_2.dat";
    private string saveSlot3 = "level_slot_3.dat";

    // ˳�� ���� ���� (�������� UI)
    private float currentLevelTimeLimit = 60f;

    void Awake()
    {
        // �������� ��� ��������� ���������� ���������
        cameraMovement = GetComponent<EditorCameraMovement>();
        if (cameraMovement == null) cameraMovement = gameObject.AddComponent<EditorCameraMovement>();

        objectPlacement = GetComponent<EditorObjectPlacement>();
        if (objectPlacement == null) objectPlacement = gameObject.AddComponent<EditorObjectPlacement>();

        uiHandler = GetComponent<EditorUIHandler>();
        if (uiHandler == null) uiHandler = gameObject.AddComponent<EditorUIHandler>();

        saveLoadManager = GetComponent<LevelSaveLoadManager>();
        if (saveLoadManager == null) saveLoadManager = gameObject.AddComponent<LevelSaveLoadManager>();
    }

    void Start()
    {
        // �������� �������� �� ����������� ���������
        cameraMovement.Init(GetComponent<Camera>(), editorPlaneCollider);
        objectPlacement.Init(this, GetComponent<Camera>()); // �������� ��������� �� LevelEditor �� ������
        uiHandler.Init(this); // �������� ��������� �� LevelEditor
        saveLoadManager.Init(this); // �������� ��������� �� LevelEditor

        if (levelObjectsParent == null)
        {
            levelObjectsParent = new GameObject("LevelObjects");
        }

        // ���������� ��� ��� ���������
        Time.timeScale = 0f;

        Debug.Log("�������� ���� ������������.");
    }

    void Update()
    {
        // �������� �� ��������/�������� ���� �����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiHandler.TogglePauseMenu();
        }

        // ���� ���� ����� �������, �� ���������� ���� ��������
        if (uiHandler.IsPauseMenuOpen())
        {
            return;
        }

        // ������� �������� ���������� ��������� ����������
        cameraMovement.HandleInput();
        objectPlacement.HandleInput();
    }

    // --- ������ ��� ������� �� ����� �� ������� ������� ����� ��������� ---

    public GameObject GetPrefabToSpawn(int objectNumber)
    {
        switch (objectNumber)
        {
            case 1: return startAsteroidPrefab;
            case 2: return (randomAsteroidPrefabs != null && randomAsteroidPrefabs.Count > 0) ? randomAsteroidPrefabs[Random.Range(0, randomAsteroidPrefabs.Count)] : null;
            case 3: return plasmaGunAsteroidPrefab;
            case 4: return laserSMGAsteroidPrefab;
            case 5: return ammo1AsteroidPrefab;
            case 6: return ammo2AsteroidPrefab;
            case 7: return healthPackPrefab;
            case 8: return turretPrefab;
            case 9: return ufoSpawnerPrefab;
            case 0: return finishAsteroidPrefab;
            default:
                Debug.LogWarning("������� ����� ��� ��������� ��'����: " + objectNumber);
                return null;
        }
    }

    public GameObject GetPrefabByName(string prefabName)
    {
        if (startAsteroidPrefab != null && startAsteroidPrefab.name == prefabName) return startAsteroidPrefab;
        if (finishAsteroidPrefab != null && finishAsteroidPrefab.name == prefabName) return finishAsteroidPrefab;
        if (plasmaGunAsteroidPrefab != null && plasmaGunAsteroidPrefab.name == prefabName) return plasmaGunAsteroidPrefab;
        if (laserSMGAsteroidPrefab != null && laserSMGAsteroidPrefab.name == prefabName) return laserSMGAsteroidPrefab;
        if (ammo1AsteroidPrefab != null && ammo1AsteroidPrefab.name == prefabName) return ammo1AsteroidPrefab;
        if (ammo2AsteroidPrefab != null && ammo2AsteroidPrefab.name == prefabName) return ammo2AsteroidPrefab;
        if (healthPackPrefab != null && healthPackPrefab.name == prefabName) return healthPackPrefab;
        if (turretPrefab != null && turretPrefab.name == prefabName) return turretPrefab;
        if (ufoSpawnerPrefab != null && ufoSpawnerPrefab.name == prefabName) return ufoSpawnerPrefab;

        if (randomAsteroidPrefabs != null)
        {
            foreach (var prefab in randomAsteroidPrefabs)
            {
                if (prefab != null && prefab.name == prefabName) return prefab;
            }
        }
        Debug.LogWarning($"������ � ��'�� '{prefabName}' �� ��������.");
        return null;
    }

    // ������, �� ������������ � UI ��� ������ �����������
    public void SetTimeLimit(float time)
    {
        currentLevelTimeLimit = time;
        uiHandler.UpdateTimeLimitText(currentLevelTimeLimit); // ��������� ����� ����� UIHandler
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
            // ���������� ������� ��'���� ���������� �������
            foreach (Transform child in levelObjectsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        // ������� ��������� �� ��������� ��'����
        startAsteroidInstance = null;
        finishAsteroidInstance = null;
        uiHandler.ClearError();
        Debug.Log("г���� �������.");
    }

    // ������ ��� ����������� ������� (������������ ������ �����������)
    public void ShowError(string message)
    {
        uiHandler.ShowError(message);
    }

    public void ClearError()
    {
        uiHandler.ClearError();
    }

    // �������� ������ ������� �� save file paths
    public string GetSaveFilePath(int slot)
    {
        string fileName;
        switch (slot)
        {
            case 1: fileName = saveSlot1; break;
            case 2: fileName = saveSlot2; break;
            case 3: fileName = saveSlot3; break;
            default:
                Debug.LogError("������� ���� ��� ����������/������������: " + slot);
                uiHandler.ShowError("������� ���� ����������.");
                return null;
        }
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}