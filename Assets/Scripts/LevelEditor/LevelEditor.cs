using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class LevelEditor : MonoBehaviour
{
    [Header("Префаби об'єктів")]
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

    [Header("Налаштування редактора")]
    public GameObject levelObjectsParent;
    public float gridSpacing = 1f;
    public float randomHeightRange = 0.5f; // Діапазон випадкової висоти

    [Header("UI Меню Паузи")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    private int currentObjectToPlace = 2;
    private Camera editorCamera;
    private float levelTimeLimit = 60f;

    private string saveSlot1 = "level_slot_1.dat";
    private string saveSlot2 = "level_slot_2.dat";
    private string saveSlot3 = "level_slot_3.dat";

    private GameObject startAsteroidInstance = null;
    private GameObject finishAsteroidInstance = null;

    private void Start()
    {
        editorCamera = GetComponent<Camera>();
        if (editorCamera == null)
        {
            Debug.LogError("Камера редактора не знайдена!");
            enabled = false;
        }

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        if (timeLimitSlider != null)
        {
            timeLimitSlider.minValue = 30f;
            timeLimitSlider.maxValue = 120f;
            timeLimitSlider.value = levelTimeLimit;
            UpdateTimeLimitText(levelTimeLimit);
            timeLimitSlider.onValueChanged.AddListener(UpdateTimeLimit);
        }

        if (errorText != null)
        {
            errorText.text = "";
        }

        if (levelObjectsParent == null)
        {
            levelObjectsParent = new GameObject("LevelObjects");
        }

        // Забезпечуємо статичний час у редакторі
        Time.timeScale = 0f;
    }

    private void Update()
    {
        // Перевірка видалення об'єкта правою кнопкою миші
        if (Input.GetMouseButtonDown(1) && pauseMenuUI != null && !pauseMenuUI.activeSelf)
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(levelObjectsParent.transform))
                {
                    GameObject objectToDelete = hit.collider.gameObject;

                    // Перевірка, чи видаляється стартовий або фінішний астероїд
                    if (objectToDelete == startAsteroidInstance)
                    {
                        startAsteroidInstance = null;
                    }
                    else if (objectToDelete == finishAsteroidInstance)
                    {
                        finishAsteroidInstance = null;
                    }

                    Destroy(objectToDelete);
                }
            }
        }

        // Розміщення об'єкта лівою кнопкою миші
        if (Input.GetMouseButtonDown(0) && pauseMenuUI != null && !pauseMenuUI.activeSelf)
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.CompareTag("EditorPlane"))
                {
                    Vector3 placementPosition = hit.point;
                    placementPosition.x = Mathf.Round(placementPosition.x / gridSpacing) * gridSpacing;
                    placementPosition.z = Mathf.Round(placementPosition.z / gridSpacing) * gridSpacing;
                    placementPosition.y = Random.Range(0f, randomHeightRange); // Випадкова висота

                    GameObject prefabToSpawn = GetPrefabToSpawn(currentObjectToPlace);

                    if (prefabToSpawn != null)
                    {
                        ObjectRadius newObjectRadius = prefabToSpawn.GetComponent<ObjectRadius>();
                        if (newObjectRadius != null)
                        {
                            bool canPlace = true;
                            foreach (Transform child in levelObjectsParent.transform)
                            {
                                ObjectRadius existingObjectRadius = child.GetComponent<ObjectRadius>();
                                if (existingObjectRadius != null)
                                {
                                    float distance = Vector3.Distance(placementPosition, child.position);
                                    if (distance < newObjectRadius.radius + existingObjectRadius.radius)
                                    {
                                        canPlace = false;
                                        ShowError("Об'єкти перетинаються. Розміщення неможливе.");
                                        break;
                                    }
                                }
                            }

                            if (canPlace)
                            {
                                if ((currentObjectToPlace == 1 && startAsteroidInstance != null) || (currentObjectToPlace == 0 && finishAsteroidInstance != null))
                                {
                                    canPlace = false;
                                    ShowError("Стартовий та фінішний астероїди можуть бути лише в одному екземплярі.");
                                }
                            }

                            if (canPlace)
                            {
                                Quaternion randomYRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f); // Випадкове обертання навколо Y
                                GameObject newObject = Instantiate(prefabToSpawn, placementPosition, Quaternion.identity, levelObjectsParent.transform);
                                newObject.transform.rotation = Quaternion.Euler(90f, randomYRotation.eulerAngles.y, 0f); // Строго вверх з випадковим обертанням по Y
                                if (currentObjectToPlace == 1) startAsteroidInstance = newObject;
                                if (currentObjectToPlace == 0) finishAsteroidInstance = newObject;
                                ClearError();
                            }
                        }
                        else
                        {
                            Quaternion randomYRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f); // Випадкове обертання навколо Y
                            GameObject newObject = Instantiate(prefabToSpawn, placementPosition, Quaternion.identity, levelObjectsParent.transform);
                            newObject.transform.rotation = Quaternion.Euler(90f, randomYRotation.eulerAngles.y, 0f); // Строго вверх з випадковим обертанням по Y
                            if (currentObjectToPlace == 1) startAsteroidInstance = prefabToSpawn;
                            if (currentObjectToPlace == 0) finishAsteroidInstance = prefabToSpawn;
                            ClearError();
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentObjectToPlace = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentObjectToPlace = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentObjectToPlace = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentObjectToPlace = 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentObjectToPlace = 5;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentObjectToPlace = 6;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentObjectToPlace = 7;
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentObjectToPlace = 8;
        if (Input.GetKeyDown(KeyCode.Alpha9)) currentObjectToPlace = 9;
        if (Input.GetKeyDown(KeyCode.Alpha0)) currentObjectToPlace = 0;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private GameObject GetPrefabToSpawn(int objectNumber)
    {
        switch (objectNumber)
        {
            case 1: return startAsteroidPrefab;
            case 2: return randomAsteroidPrefabs.Count > 0 ? randomAsteroidPrefabs[Random.Range(0, randomAsteroidPrefabs.Count)] : null;
            case 3: return plasmaGunAsteroidPrefab;
            case 4: return laserSMGAsteroidPrefab;
            case 5: return ammo1AsteroidPrefab;
            case 6: return ammo2AsteroidPrefab;
            case 7: return healthPackPrefab;
            case 8: return turretPrefab;
            case 9: return ufoSpawnerPrefab;
            case 0: return finishAsteroidPrefab;
            default:
                Debug.LogWarning("Невідома цифра для розміщення об'єкта.");
                return null;
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(!pauseMenuUI.activeSelf);
        }
    }

    public void UpdateTimeLimit(float time)
    {
        levelTimeLimit = time;
        UpdateTimeLimitText(levelTimeLimit);
    }

    private void UpdateTimeLimitText(float time)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "Час: " + time.ToString("F0") + " сек.";
        }
    }

    public void SaveLevel(int slot)
    {
        if (startAsteroidInstance == null || finishAsteroidInstance == null)
        {
            ShowError("На рівні повинні бути присутні стартовий та фінішний астероїди.");
            return;
        }
        ClearError();

        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return;

        LevelData levelData = new LevelData();
        levelData.timeLimit = levelTimeLimit;
        levelData.placedObjects = new List<ObjectData>();

        foreach (Transform child in levelObjectsParent.transform)
        {
            ObjectData objectData = new ObjectData();
            objectData.prefabName = child.gameObject.name.Replace("(Clone)", "");
            objectData.position = child.position;
            objectData.rotation = child.rotation;
            levelData.placedObjects.Add(objectData);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(filePath);
        formatter.Serialize(file, levelData);
        file.Close();

        Debug.Log("Рівень збережено в слот " + slot);
    }

    public void LoadLevel(int slot)
    {
        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.Log("Збереження у слоті " + slot + " не знайдено.");
            return;
        }

        ClearLevel();
        startAsteroidInstance = null;
        finishAsteroidInstance = null;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);
        LevelData loadedData = (LevelData)formatter.Deserialize(file);
        file.Close();

        levelTimeLimit = loadedData.timeLimit;
        timeLimitSlider.value = levelTimeLimit;
        UpdateTimeLimitText(levelTimeLimit);

        foreach (ObjectData objectData in loadedData.placedObjects)
        {
            GameObject prefabToInstantiate = GetPrefabByName(objectData.prefabName);
            if (prefabToInstantiate != null)
            {
                GameObject instantiatedObject = Instantiate(prefabToInstantiate, objectData.position, objectData.rotation, levelObjectsParent.transform);
                if (prefabToInstantiate == startAsteroidPrefab) startAsteroidInstance = instantiatedObject;
                if (prefabToInstantiate == finishAsteroidPrefab) finishAsteroidInstance = instantiatedObject;
            }
            else
            {
                Debug.LogWarning("Префаб " + objectData.prefabName + " не знайдено!");
            }
        }

        Debug.Log("Рівень завантажено зі слоту " + slot);
        ClearError();
    }

    public void ClearLevel()
    {
        foreach (Transform child in levelObjectsParent.transform)
        {
            Destroy(child.gameObject);
        }
        startAsteroidInstance = null;
        finishAsteroidInstance = null;
        ClearError();
    }

    private string GetSaveFilePath(int slot)
    {
        switch (slot)
        {
            case 1: return Application.persistentDataPath + "/" + saveSlot1;
            case 2: return Application.persistentDataPath + "/" + saveSlot2;
            case 3: return Application.persistentDataPath + "/" + saveSlot3;
            default:
                Debug.LogError("Невірний слот: " + slot);
                return null;
        }
    }

    private GameObject GetPrefabByName(string prefabName)
    {
        if (startAsteroidPrefab != null && startAsteroidPrefab.name == prefabName) return startAsteroidPrefab;
        if (finishAsteroidPrefab != null && finishAsteroidPrefab.name == prefabName) return finishAsteroidPrefab;
        foreach (var prefab in randomAsteroidPrefabs) if (prefab != null && prefab.name == prefabName) return prefab;
        if (plasmaGunAsteroidPrefab != null && plasmaGunAsteroidPrefab.name == prefabName) return plasmaGunAsteroidPrefab;
        if (laserSMGAsteroidPrefab != null && laserSMGAsteroidPrefab.name == prefabName) return laserSMGAsteroidPrefab;
        if (ammo1AsteroidPrefab != null && ammo1AsteroidPrefab.name == prefabName) return ammo1AsteroidPrefab;
        if (ammo2AsteroidPrefab != null && ammo2AsteroidPrefab.name == prefabName) return ammo2AsteroidPrefab;
        if (healthPackPrefab != null && healthPackPrefab.name == prefabName) return healthPackPrefab;
        if (turretPrefab != null && turretPrefab.name == prefabName) return turretPrefab;
        if (ufoSpawnerPrefab != null && ufoSpawnerPrefab.name == prefabName) return ufoSpawnerPrefab;
        return null;
    }

    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
        }
        Debug.LogError(message);
    }

    private void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
    }
}

[System.Serializable]
public class ObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class LevelData
{
    public float timeLimit;
    public List<ObjectData> placedObjects;
}