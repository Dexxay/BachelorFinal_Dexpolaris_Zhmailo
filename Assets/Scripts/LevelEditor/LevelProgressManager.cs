using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    private const string MaxLevelReachedKey = "MaxLevelReached";
    private const string CampaignLevel1Name = "campaign_1";
    private const string CampaignLevel2Name = "campaign_2";

    public static void UnlockNextLevel(string completedLevelName)
    {
        int maxLevelReached = PlayerPrefs.GetInt(MaxLevelReachedKey, 0);

        if (completedLevelName == CampaignLevel1Name && maxLevelReached < 1)
        {
            PlayerPrefs.SetInt(MaxLevelReachedKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Level 2 unlocked");
        }
        else if (completedLevelName == CampaignLevel2Name && maxLevelReached < 2)
        {
            PlayerPrefs.SetInt(MaxLevelReachedKey, 2);
            PlayerPrefs.Save();
            Debug.Log("Level 3 unlocked");
        }
    }
}