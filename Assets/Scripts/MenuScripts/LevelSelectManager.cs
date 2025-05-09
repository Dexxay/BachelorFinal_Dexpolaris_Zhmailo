using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private List<Button> campaignLevelButtons;
    [SerializeField] private LevelProgressManager levelProgressManager;

    [SerializeField] private Button customLevelSlot1Button;
    [SerializeField] private Button customLevelSlot2Button;
    [SerializeField] private Button customLevelSlot3Button;

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string CustomLevelSlot1Name = "level_slot_1.dat";
    private const string CustomLevelSlot2Name = "level_slot_2.dat";
    private const string CustomLevelSlot3Name = "level_slot_3.dat";

    void Start()
    {
        if (levelProgressManager == null)
        {
            Debug.LogError("LevelProgressManager is not assigned in LevelSelectManager!");
            return;
        }

        SetupCampaignButtons();
        SetupCustomLevelButtons();
    }

    void SetupCampaignButtons()
    {
        List<string> campaignLevels = levelProgressManager.GetCampaignLevels();

        if (campaignLevelButtons.Count != campaignLevels.Count)
        {
            Debug.LogError("Mismatch between the number of campaign level buttons assigned in Inspector and the number of levels in LevelProgressManager!");
        }

        int maxButtons = Mathf.Min(campaignLevelButtons.Count, campaignLevels.Count);

        for (int i = 0; i < maxButtons; i++)
        {
            string levelName = campaignLevels[i];
            Button levelButton = campaignLevelButtons[i];

            if (levelButton == null)
            {
                Debug.LogError($"Campaign level button at index {i} is not assigned in the Inspector!");
                continue;
            }

            levelButton.onClick.RemoveAllListeners();
            string currentLevelName = levelName;
            levelButton.onClick.AddListener(() => LoadCampaignLevel(currentLevelName));

            if (levelProgressManager.IsLevelUnlocked(levelName))
            {
                levelButton.interactable = true;
            }
            else
            {
                levelButton.interactable = false;
            }
        }
    }

    void SetupCustomLevelButtons()
    {
        string slot1Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot1Name);
        string slot2Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot2Name);
        string slot3Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot3Name);

        if (customLevelSlot1Button != null)
        {
            if (File.Exists(slot1Path))
            {
                customLevelSlot1Button.interactable = true;
                customLevelSlot1Button.onClick.RemoveAllListeners();
                customLevelSlot1Button.onClick.AddListener(() => LoadCustomLevel(1));
            }
            else
            {
                customLevelSlot1Button.interactable = false;
            }
        }
        else
        {
            Debug.LogError("customLevelSlot1Button is not assigned in the Inspector.");
        }


        if (customLevelSlot2Button != null)
        {
            if (File.Exists(slot2Path))
            {
                customLevelSlot2Button.interactable = true;
                customLevelSlot2Button.onClick.RemoveAllListeners();
                customLevelSlot2Button.onClick.AddListener(() => LoadCustomLevel(2));
            }
            else
            {
                customLevelSlot2Button.interactable = false;
            }
        }
        else
        {
            Debug.LogError("customLevelSlot2Button is not assigned in the Inspector.");
        }


        if (customLevelSlot3Button != null)
        {
            if (File.Exists(slot3Path))
            {
                customLevelSlot3Button.interactable = true;
                customLevelSlot3Button.onClick.RemoveAllListeners();
                customLevelSlot3Button.onClick.AddListener(() => LoadCustomLevel(3));
            }
            else
            {
                customLevelSlot3Button.interactable = false;
            }
        }
        else
        {
            Debug.LogError("customLevelSlot3Button is not assigned in the Inspector.");
        }
    }

    void LoadCampaignLevel(string levelName)
    {
        if (levelProgressManager.IsLevelUnlocked(levelName))
        {
            PlayerPrefs.SetString("CurrentLevelType", "Campaign");
            PlayerPrefs.SetString("CurrentLevelName", levelName);
            PlayerPrefs.Save();
            if (string.IsNullOrEmpty(gameSceneName))
            {
                Debug.LogError("gameSceneName is not set in the Inspector.");
                return;
            }
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning($"Attempted to load locked campaign level: {levelName}");
        }
    }

    void LoadCustomLevel(int slotNumber)
    {
        PlayerPrefs.SetString("CurrentLevelType", "Custom");
        PlayerPrefs.SetInt("CurrentLevelSlot", slotNumber);
        PlayerPrefs.Save();
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("gameSceneName is not set in the Inspector.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("mainMenuSceneName is not set in the Inspector. Cannot load Main Menu.");
            return;
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}