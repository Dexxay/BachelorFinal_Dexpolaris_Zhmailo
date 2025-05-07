// GameLevelLoader.cs
using UnityEngine;
using System.Collections.Generic;

public class GameLevelLoader : MonoBehaviour
{
    public LevelSaveLoadManager levelSaveLoadManager;
    public List<GameObject> availablePrefabs;
    public Transform levelObjectsParentTransform;
    public int levelSlotToLoad = 1;

    void Start()
    {
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

        // Assuming you have added tags "StartAsteroid" and "FinishAsteroid"
        // to your start and finish asteroid prefabs in the Unity Editor,
        // and that the LoadLevelForGame method correctly assigns these tags
        // to the instantiated objects based on the saved data.

        foreach (Transform child in levelObjectsParentTransform)
        {
            if (child.CompareTag("StartAsteroid"))
            {
                startAsteroid = child.gameObject;
                // Debug.Log("Start asteroid found in game."); // Optional log
            }
            if (child.CompareTag("FinishAsteroid"))
            {
                finishAsteroid = child.gameObject;
                // Debug.Log("Finish asteroid found in game."); // Optional log
            }
        }


        if (startAsteroid != null)
        {
            // You can now pass the reference to the start asteroid to your player controller for spawning
            // Example: PlayerController.Instance.SetSpawnPoint(startAsteroid.transform.position);
        }
        else
        {
            Debug.LogWarning("Start asteroid not found in loaded level for game (looking for tag 'StartAsteroid').");
        }

        if (finishAsteroid != null)
        {
            // You can now pass the reference to the finish asteroid to your game manager for win condition check
            // Example: GameManager.Instance.SetFinishPoint(finishAsteroid.transform);
        }
        else
        {
            Debug.LogWarning("Finish asteroid not found in loaded level for game (looking for tag 'FinishAsteroid').");
        }

    }

    // Method to call level loading from game UI (e.g., buttons in a menu)
    public void LoadSelectedLevel(int slot)
    {
        levelSlotToLoad = slot;
        // You might want to reload the scene or simply clear and load the new level
        // If the scene is not reloaded, you need to clear the previous level first
        if (levelObjectsParentTransform != null)
        {
            // Create a list of children to destroy to avoid modifying the collection during iteration
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in levelObjectsParentTransform)
            {
                childrenToDestroy.Add(child.gameObject);
            }
            // Destroy objects after iterating through the list
            foreach (GameObject child in childrenToDestroy)
            {
                Destroy(child);
            }
        }
        Start(); // Or call a separate method containing the loading logic
    }
}