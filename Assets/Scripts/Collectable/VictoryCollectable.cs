using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryCollectable : Collectable
{
    private UIManager uiManager;
    private LevelProgressManager levelProgressManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found on scene!");
        }

        levelProgressManager = FindObjectOfType<LevelProgressManager>();
        if (levelProgressManager == null)
        {
            Debug.LogError("LevelProgressManager not found on scene!");
        }
    }

    public override void OnBeingCollectedBy(MainPlayer character)
    {
        character.OnPlayerWon();
        uiManager?.ShowVictoryScreen();

        string currentLevelType = PlayerPrefs.GetString("CurrentLevelType", "");
        string currentLevelName = PlayerPrefs.GetString("CurrentLevelName", "");

        if (currentLevelType == "Campaign")
        {
            if (levelProgressManager != null)
            {
                levelProgressManager.UnlockNextLevel(currentLevelName);
            }
            else
            {
                Debug.LogError("LevelProgressManager is null. Cannot unlock next level.");
            }
        }

        Destroy(gameObject);
    }
}