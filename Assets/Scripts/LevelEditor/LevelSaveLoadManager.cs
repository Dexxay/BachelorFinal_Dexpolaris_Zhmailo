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

    public string saveSlot1 = "level_slot_1.dat";
    public string saveSlot2 = "level_slot_2.dat";
    public string saveSlot3 = "level_slot_3.dat";


    public void Init(LevelEditor editor)
    {
        levelEditor = editor;
        if (levelEditor == null)
        {
            Debug.LogError("LevelSaveLoadManager did not receive a reference to LevelEditor during Init!");
        }
    }

    public string GetSaveFilePath(int slot)
    {
        string fileName;
        switch (slot)
        {
            case 1: fileName = saveSlot1; break;
            case 2: fileName = saveSlot2; break;
            case 3: fileName = saveSlot3; break;
            default:
                Debug.LogError($"Invalid save slot: {slot}");
                return null;
        }
        return Path.Combine(Application.persistentDataPath, fileName);
    }


    public void SaveLevel(int slot)
    {
        if (levelEditor == null)
        {
            Debug.LogError("LevelEditor is not available. Cannot save level.");
            return;
        }

        if (levelEditor.startAsteroidInstance == null || levelEditor.finishAsteroidInstance == null)
        {
            levelEditor.ShowMessage("Start and finish asteroids must be present on the level to save.", true);
            return;
        }
        levelEditor.ClearMessage();

        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return;

        LevelData levelData = new LevelData();
        levelData.timeLimit = levelEditor.GetTimeLimit();
        levelData.placedObjects = new List<ObjectData>();

        if (levelEditor.levelObjectsParent != null)
        {
            foreach (Transform child in levelEditor.levelObjectsParent.transform)
            {
                PlacedObjectInfo objectInfo = child.GetComponent<PlacedObjectInfo>();
                if (objectInfo != null)
                {
                    ObjectData objectData = new ObjectData
                    {
                        prefabName = objectInfo.originalPrefabName,
                        position = child.position,
                        rotation = child.rotation,
                        isStartAsteroid = (child.gameObject == levelEditor.startAsteroidInstance),
                        isFinishAsteroid = (child.gameObject == levelEditor.finishAsteroidInstance)
                    };
                    levelData.placedObjects.Add(objectData);
                }
                else
                {
                    Debug.LogWarning($"Object '{child.name}' is missing PlacedObjectInfo component. It will not be saved.");
                }
            }
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(fileStream, levelData);
            fileStream.Close();

            Debug.Log($"Level successfully saved to slot {slot}: {filePath}");
            if (levelEditor != null) levelEditor.ShowMessage($"Level saved to slot {slot}", false);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving level to slot {slot}: {e.Message}");
            if (levelEditor != null) levelEditor.ShowMessage($"Error saving level to slot {slot}", true);
        }
    }

    public void LoadLevel(int slot)
    {
        if (levelEditor == null)
        {
            Debug.LogError("LevelEditor is not available. Cannot load level in editor.");
            return;
        }

        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return;

        if (!File.Exists(filePath))
        {
            levelEditor.ShowMessage($"Save file for slot {slot} not found.", true);
            Debug.LogWarning($"Save file for slot {slot} not found: {filePath}");
            return;
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            LevelData loadedData = formatter.Deserialize(fileStream) as LevelData;
            fileStream.Close();

            if (loadedData == null)
            {
                levelEditor.ShowMessage($"Failed to load level data from slot {slot}.", true);
                Debug.LogError($"Failed to deserialize level data from file: {filePath}");
                return;
            }

            levelEditor.ClearLevel();

            levelEditor.SetTimeLimit(loadedData.timeLimit);

            if (levelEditor.levelObjectsParent == null)
            {
                Debug.LogError("Level Objects parent object not found during loading!");
                levelEditor.ShowMessage("Loading Error: Level Objects parent object not found.", true);
                return;
            }

            GameObject tempStart = null;
            GameObject tempFinish = null;


            foreach (ObjectData objectData in loadedData.placedObjects)
            {
                GameObject prefabToInstantiate = levelEditor.GetPrefabByName(objectData.prefabName);

                if (prefabToInstantiate != null)
                {
                    GameObject instantiatedObject = Instantiate(prefabToInstantiate, objectData.position, objectData.rotation, levelEditor.levelObjectsParent.transform);

                    PlacedObjectInfo objectInfo = instantiatedObject.AddComponent<PlacedObjectInfo>();
                    objectInfo.originalPrefabName = objectData.prefabName;


                    if (objectData.isStartAsteroid)
                    {
                        tempStart = instantiatedObject;
                    }
                    if (objectData.isFinishAsteroid)
                    {
                        tempFinish = instantiatedObject;
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab with name '{objectData.prefabName}' not found when loading editor level! Object will not be created.");
                }
            }

            levelEditor.startAsteroidInstance = tempStart;
            levelEditor.finishAsteroidInstance = tempFinish;


            Debug.Log($"Level loaded from slot {slot}");
            if (levelEditor != null) levelEditor.ShowMessage($"Level loaded from slot {slot}", false);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading level from slot {slot}: {e.Message}");
            if (levelEditor != null) levelEditor.ShowMessage($"Error loading level to slot {slot}", true);
        }
    }

    public LevelData LoadLevelForGame(int slot, Transform parentTransform, List<GameObject> availablePrefabs)
    {
        string filePath = GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError($"Invalid file path for slot {slot}.");
            return null;
        }


        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Save file for slot {slot} not found for game loading: {filePath}");
            return null;
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            LevelData loadedData = formatter.Deserialize(fileStream) as LevelData;
            fileStream.Close();

            if (loadedData == null)
            {
                Debug.LogError($"Failed to load level data from slot {slot} for game.");
                return null;
            }

            if (parentTransform != null)
            {
                List<GameObject> childrenToDestroy = new List<GameObject>();
                foreach (Transform child in parentTransform)
                {
                    childrenToDestroy.Add(child.gameObject);
                }
                foreach (GameObject child in childrenToDestroy)
                {
                    Destroy(child);
                }
            }

            foreach (ObjectData objectData in loadedData.placedObjects)
            {
                GameObject prefabToInstantiate = FindPrefabByName(objectData.prefabName, availablePrefabs);

                if (prefabToInstantiate != null)
                {
                    GameObject instantiatedObject = Instantiate(prefabToInstantiate, objectData.position, objectData.rotation, parentTransform);

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
            Debug.LogWarning("Available prefabs list is null or empty in LevelSaveLoadManager. Cannot find prefab by name.");
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