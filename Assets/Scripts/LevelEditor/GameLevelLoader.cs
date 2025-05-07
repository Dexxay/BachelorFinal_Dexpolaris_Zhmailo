using UnityEngine;
using System.Collections.Generic;

public class GameLevelLoader : MonoBehaviour
{
    public LevelSaveLoadManager levelSaveLoadManager;
    public List<GameObject> availablePrefabs;
    public Transform levelObjectsParentTransform;
    public int levelSlotToLoad = 1;

    private MainPlayer player;

    void Start()
    {
        player = FindFirstObjectByType<MainPlayer>();

        if (levelSaveLoadManager == null)
        {
            Debug.LogError("LevelSaveLoadManager not assigned in GameLevelLoader!");
            return;
        }
        if (levelObjectsParentTransform == null)
        {
            Debug.LogError("LevelObjectsParentTransform not assigned in GameLevelLoader! Level objects will be created at the scene root.");
        }
        if (availablePrefabs == null || availablePrefabs.Count == 0)
        {
            Debug.LogError("Available prefabs list is empty or not assigned in GameLevelLoader! Level objects loading will not be possible.");
            return;
        }


        LevelData loadedLevelData = levelSaveLoadManager.LoadLevelForGame(levelSlotToLoad, levelObjectsParentTransform, availablePrefabs);

        if (loadedLevelData != null)
        {
            Debug.Log($"Level loaded successfully. Time limit: {loadedLevelData.timeLimit} sec.");
            FindStartAndFinishAsteroids(loadedLevelData);
        }
        else
        {
            Debug.LogError("Failed to load level for game.");
        }
    }

    void FindStartAndFinishAsteroids(LevelData loadedLevelData)
    {
        if (levelObjectsParentTransform == null)
        {
            Debug.LogWarning("levelObjectsParentTransform not assigned. Cannot find start/finish asteroids by parent object.");
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


        if (startAsteroid != null)
        {
            player.transform.position = startAsteroid.transform.position + new Vector3(0,7,0);
            Debug.Log("Player teleported to " + startAsteroid.transform.position);

        }
        else
        {
            Debug.LogWarning("Start asteroid not found in loaded level for game (looking for tag 'StartAsteroid').");
        }
        if (finishAsteroid != null)
        {

        }
        else
        {
            Debug.LogWarning("Finish asteroid not found in loaded level for game (looking for tag 'FinishAsteroid').");
        }
    }

    public void LoadSelectedLevel(int slot)
    {
        levelSlotToLoad = slot;
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
        Start();
    }
}