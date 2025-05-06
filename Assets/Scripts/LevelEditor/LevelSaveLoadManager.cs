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
            Debug.LogError("LevelSaveLoadManager �� ������� ��������� �� LevelEditor!");
            enabled = false;
        }
    }

    public void SaveLevel(int slot)
    {
        if (levelEditor.startAsteroidInstance == null || levelEditor.finishAsteroidInstance == null)
        {
            levelEditor.ShowError("�� ��� ������ ���� ������� ��������� �� ������� �������� ��� ����������.");
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
            Debug.Log($"г���� ��������� � ���� {slot} �� ������: {filePath}");
            levelEditor.ShowError($"г���� ��������� � ���� {slot}");
        }
        catch (System.Exception e)
        {
            levelEditor.ShowError($"������� ���������� ���� � ���� {slot}: {e.Message}");
            Debug.LogError($"������� ���������� ����: {e.ToString()}");
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
            levelEditor.ShowError($"���������� � ���� {slot} �� ��������.");
            Debug.LogWarning($"���� ���������� �� �������� �� ������: {filePath}");
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
            levelEditor.ShowError($"������� ������������ ���� � ����� {slot}: {e.Message}");
            Debug.LogError($"������� ������������ ����: {e.ToString()}"); 
                                                                           
        }
        finally
        {
            if (file != null) file.Close();
        }


        // ����������, �� ������� ������������� ���
        if (loadedData == null)
        {
            levelEditor.ShowError($"�� ������� ������������� ��� ���� � ����� {slot}. ���� ���� ���� �����������.");
            return;
        }


        levelEditor.SetTimeLimit(loadedData.timeLimit);

        if (levelEditor.levelObjectsParent == null)
        {
            Debug.LogError("����������� ��'��� ��� ���� LevelObjects �� �������� �� ��� ������������!");
            levelEditor.ShowError("������� ������������: �� �������� ������������ ��'���� ��� ��'���� ����.");
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
                Debug.LogWarning($"������ � ��'�� '{objectData.prefabName}' �� �������� �� ��� ������������!");
            }
        }

        Debug.Log($"г���� ����������� � ����� {slot}");
        levelEditor.ShowError($"г���� ����������� � ����� {slot}"); 
    }
}