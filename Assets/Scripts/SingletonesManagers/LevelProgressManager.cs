using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelProgressManager : MonoBehaviour
{
    private static LevelProgressManager instance;

    private const string MaxLevelIndexReachedKey = "MaxLevelIndexReached";
    public static LevelProgressManager Instance => instance;

    public List<string> campaignLevels = new List<string>()
    {
        "campaign_1",
        "campaign_2",
        "campaign_3",
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        if (campaignLevels == null || campaignLevels.Count == 0)
        {
            Debug.LogError("Campaign levels list is empty in LevelProgressManager!");
        }

        if (!PlayerPrefs.HasKey(MaxLevelIndexReachedKey))
        {
            PlayerPrefs.SetInt(MaxLevelIndexReachedKey, 0);
            PlayerPrefs.Save();
        }
    }

    public void UnlockNextLevel(string completedLevelName)
    {
        int maxLevelIndexReached = PlayerPrefs.GetInt(MaxLevelIndexReachedKey, 0);

        int completedLevelIndex = campaignLevels.IndexOf(completedLevelName);

        if (completedLevelIndex != -1)
        {
            int nextLevelIndexToUnlock = completedLevelIndex + 1;

            if (nextLevelIndexToUnlock >= campaignLevels.Count)
            {
                Debug.Log("Completed the last campaign level.");
            }
            else
            {
                if (nextLevelIndexToUnlock > maxLevelIndexReached)
                {
                    PlayerPrefs.SetInt(MaxLevelIndexReachedKey, nextLevelIndexToUnlock);
                    PlayerPrefs.Save();
                    Debug.Log($"Level '{campaignLevels[nextLevelIndexToUnlock]}' unlocked.");
                }
                else
                {
                    Debug.Log($"Level '{campaignLevels[nextLevelIndexToUnlock]}' is already unlocked.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Completed level '{completedLevelName}' not found in the campaign levels list.");
        }
    }

    public bool IsLevelUnlocked(string levelName)
    {
        if (campaignLevels == null || campaignLevels.Count == 0)
        {
            Debug.LogError("Campaign levels list is empty in LevelProgressManager!");
            return false;
        }

        int levelIndex = campaignLevels.IndexOf(levelName);
        if (levelIndex == -1)
        {
            Debug.LogWarning($"Level '{levelName}' not found in the campaign levels list.");
            return true;
        }

        int maxLevelIndexReached = PlayerPrefs.GetInt(MaxLevelIndexReachedKey, 0);

        return levelIndex <= maxLevelIndexReached;
    }

    public List<string> GetCampaignLevels()
    {
        return campaignLevels;
    }

    public int GetMaxLevelIndexReached()
    {
        return PlayerPrefs.GetInt(MaxLevelIndexReachedKey, 0);
    }
}