using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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


public class LevelSaveLoadManager : MonoBehaviour
{
    private LevelEditor levelEditor;

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;
        if (levelEditor == null)
        {
            Debug.LogError("LevelSaveLoadManager не отримав посилання на LevelEditor!");
            enabled = false;
        }
    }

    public void SaveLevel(int slot)
    {
        if (levelEditor.startAsteroidInstance == null || levelEditor.finishAsteroidInstance == null)
        {
            levelEditor.ShowError("На рівні повинні бути присутні стартовий та фінішний астероїди для збереження.");
            return;
        }
        levelEditor.ClearError();

        string filePath = levelEditor.GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return; 

        LevelData levelData = new LevelData();
        levelData.timeLimit = levelEditor.GetTimeLimit();
        levelData.placedObjects = new List<ObjectData>();

        if (levelEditor.levelObjectsParent != null)
        {
            foreach (Transform child in levelEditor.levelObjectsParent.transform)
            {
                ObjectData objectData = new ObjectData();
                objectData.prefabName = child.gameObject.name.Replace("(Clone)", "").Trim();
                objectData.position = child.position;
                objectData.rotation = child.rotation;
                levelData.placedObjects.Add(objectData);
            }
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = null;

        try
        {
            file = File.Create(filePath);
            formatter.Serialize(file, levelData);
            Debug.Log($"Рівень збережено в слот {slot} за шляхом: {filePath}");
            levelEditor.ShowError($"Рівень збережено в слот {slot}");
        }
        catch (System.Exception e)
        {
            levelEditor.ShowError($"Помилка збереження рівня у слот {slot}: {e.Message}");
            Debug.LogError($"Помилка збереження рівня: {e.ToString()}");
        }
        finally
        {
            if (file != null) file.Close();
        }
    }

    public void LoadLevel(int slot)
    {
        string filePath = levelEditor.GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return;


        if (!File.Exists(filePath))
        {
            levelEditor.ShowError($"Збереження у слоті {slot} не знайдено.");
            Debug.LogWarning($"Файл збереження не знайдено за шляхом: {filePath}");
            return;
        }

        levelEditor.ClearLevel();

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = null;
        LevelData loadedData = null; 

        try
        {
            file = File.Open(filePath, FileMode.Open);
            loadedData = (LevelData)formatter.Deserialize(file); 
        }
        catch (System.Exception e)
        {
            levelEditor.ShowError($"Помилка завантаження рівня зі слоту {slot}: {e.Message}");
            Debug.LogError($"Помилка завантаження рівня: {e.ToString()}"); 
                                                                           
        }
        finally
        {
            if (file != null) file.Close();
        }


        // Перевіряємо, чи вдалося десеріалізувати дані
        if (loadedData == null)
        {
            levelEditor.ShowError($"Не вдалося десеріалізувати дані рівня зі слоту {slot}. Файл може бути пошкоджений.");
            return;
        }


        levelEditor.SetTimeLimit(loadedData.timeLimit);

        if (levelEditor.levelObjectsParent == null)
        {
            Debug.LogError("Батьківський об'єкт для рівня LevelObjects не знайдено під час завантаження!");
            levelEditor.ShowError("Помилка завантаження: Не знайдено батьківського об'єкта для об'єктів рівня.");
            return;
        }


        foreach (ObjectData objectData in loadedData.placedObjects)
        {
            GameObject prefabToInstantiate = levelEditor.GetPrefabByName(objectData.prefabName);

            if (prefabToInstantiate != null)
            {
                GameObject instantiatedObject = Instantiate(prefabToInstantiate, objectData.position, objectData.rotation, levelEditor.levelObjectsParent.transform);

                if (levelEditor.startAsteroidPrefab != null && prefabToInstantiate.name == levelEditor.startAsteroidPrefab.name)
                {
                    levelEditor.startAsteroidInstance = instantiatedObject;
                }
                else if (levelEditor.finishAsteroidPrefab != null && prefabToInstantiate.name == levelEditor.finishAsteroidPrefab.name)
                {
                    levelEditor.finishAsteroidInstance = instantiatedObject;
                }
            }
            else
            {
                Debug.LogWarning($"Префаб з ім'ям '{objectData.prefabName}' не знайдено під час завантаження!");
            }
        }

        Debug.Log($"Рівень завантажено зі слоту {slot}");
        levelEditor.ShowError($"Рівень завантажено зі слоту {slot}"); 
    }
}