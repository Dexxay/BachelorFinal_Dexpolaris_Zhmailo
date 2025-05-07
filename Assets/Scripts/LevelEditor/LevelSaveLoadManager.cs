using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v.x, v.y, v.z);
    }

    public static implicit operator Vector3(SerializableVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}

[System.Serializable]
public struct SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator SerializableQuaternion(Quaternion q)
    {
        return new SerializableQuaternion(q.x, q.y, q.z, q.w);
    }

    public static implicit operator Quaternion(SerializableQuaternion q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
}

[System.Serializable]
public class ObjectData
{
    public string prefabName;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public bool isStartAsteroid;
    public bool isFinishAsteroid;

    public ObjectData(GameObject obj, GameObject startInstance, GameObject finishInstance)
    {
        PlacedObjectInfo placedInfo = obj.GetComponent<PlacedObjectInfo>();
        if (placedInfo != null)
        {
            prefabName = placedInfo.originalPrefabName;
        }
        else
        {
            prefabName = obj.name.Replace("(Clone)", "").Trim();
            Debug.LogWarning($"Object {obj.name} does not have PlacedObjectInfo. Using object name as prefab name.");
        }

        position = obj.transform.position;
        rotation = obj.transform.rotation;
        isStartAsteroid = (obj == startInstance);
        isFinishAsteroid = (obj == finishInstance);
    }
}

[System.Serializable]
public class LevelData
{
    public List<ObjectData> objects;
    public float timeLimit;
}

public class LevelSaveLoadManager : MonoBehaviour
{
    public void SaveLevel(string filePath, GameObject levelObjectsParent, float timeLimit, GameObject startAsteroidInstance, GameObject finishAsteroidInstance)
    {
        if (levelObjectsParent == null)
        {
            Debug.LogError("Parent object for level objects is not assigned!");
            return;
        }

        LevelData levelData = new LevelData();
        levelData.timeLimit = timeLimit;
        levelData.objects = new List<ObjectData>();

        foreach (Transform child in levelObjectsParent.transform)
        {
            levelData.objects.Add(new ObjectData(child.gameObject, startAsteroidInstance, finishAsteroidInstance));
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                formatter.Serialize(stream, levelData);
            }
            Debug.Log($"Level saved to {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving level to {filePath}: {e.Message}");
        }
    }

    public LevelData LoadLevelForEditor(string filePath, GameObject levelObjectsParent, LevelEditor editorInstance)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found at {filePath}");
            return null;
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            LevelData loadedData;
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                loadedData = (LevelData)formatter.Deserialize(stream);
            }

            if (levelObjectsParent == null)
            {
                Debug.LogError("Parent transform for loaded objects not specified in LoadLevelForEditor!");
                return loadedData;
            }
            if (editorInstance == null)
            {
                Debug.LogError("LevelEditor instance not provided to LoadLevelForEditor!");
                return loadedData;
            }

            foreach (ObjectData objectData in loadedData.objects)
            {
                GameObject prefabToLoad = editorInstance.GetPrefabByName(objectData.prefabName);
                if (prefabToLoad != null)
                {
                    GameObject newObject = Instantiate(prefabToLoad, objectData.position, objectData.rotation, levelObjectsParent.transform);

                    PlacedObjectInfo objectInfo = newObject.GetComponent<PlacedObjectInfo>();
                    if (objectInfo == null)
                    {
                        objectInfo = newObject.AddComponent<PlacedObjectInfo>();
                    }
                    objectInfo.originalPrefabName = objectData.prefabName;
                }
                else
                {
                    Debug.LogWarning($"Prefab with name '{objectData.prefabName}' not found in LevelEditor! Object will not be created.");
                }
            }

            Debug.Log($"Level loaded from {filePath} for editor.");
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading level from {filePath} for editor: {e.Message}");
            return null;
        }
    }

    public bool DeleteLevel(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Level file deleted: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deleting file {filePath}: {e.Message}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"File not found, cannot delete: {filePath}");
            return false;
        }
    }

    public LevelData LoadLevelForGame(int slot, Transform parentTransform, List<GameObject> availablePrefabs)
    {
        string filePath = Path.Combine(Application.persistentDataPath, $"level_slot_{slot}.dat");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file not found at {filePath} for slot {slot}.");
            return null;
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            LevelData loadedData;
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                loadedData = (LevelData)formatter.Deserialize(stream);
            }

            if (parentTransform == null)
            {
                Debug.LogError("Parent transform for loaded objects not specified in LoadLevelForGame!");
            }
            if (availablePrefabs == null || availablePrefabs.Count == 0)
            {
                Debug.LogError("Available prefabs list is empty in LoadLevelForGame!");
                return loadedData;
            }

            foreach (ObjectData objectData in loadedData.objects)
            {
                GameObject prefabToInstantiate = FindPrefabByName(objectData.prefabName, availablePrefabs);
                if (prefabToInstantiate != null)
                {
                    GameObject instantiatedObject = Instantiate(prefabToInstantiate, (Vector3)objectData.position, (Quaternion)objectData.rotation, parentTransform);

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

            Debug.Log($"Level loaded from slot {slot} for game.");
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading level from slot {slot} for game: {e.Message}");
            return null;
        }
    }

    private GameObject FindPrefabByName(string prefabName, List<GameObject> availablePrefabs)
    {
        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogWarning("Available prefabs list is null or empty in LevelSaveLoadManager.Cannot find prefab by name.");
            return null;
        }
        foreach (var prefab in availablePrefabs)
        {
            if (prefab != null && prefab.name == prefabName)
            {
                return prefab;
            }
        }
        return null;
    }
}