using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryCollectable : Collectable
{
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found on scene!");
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
            LevelProgressManager.UnlockNextLevel(currentLevelName);
        }

        Destroy(gameObject);
    }
}