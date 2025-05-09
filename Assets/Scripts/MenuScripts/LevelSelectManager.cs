using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private Button campaignLevel1Button;
    [SerializeField] private Button campaignLevel2Button;
    [SerializeField] private Button campaignLevel3Button;

    [SerializeField] private Button customLevelSlot1Button;
    [SerializeField] private Button customLevelSlot2Button;
    [SerializeField] private Button customLevelSlot3Button;

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string CampaignLevel1Name = "campaign_1";
    private const string CampaignLevel2Name = "campaign_2";
    private const string CampaignLevel3Name = "campaign_3";

    private const string CustomLevelSlot1Name = "level_slot_1.dat";
    private const string CustomLevelSlot2Name = "level_slot_2.dat";
    private const string CustomLevelSlot3Name = "level_slot_3.dat";

    private const string MaxLevelReachedKey = "MaxLevelReached";

    void Start()
    {
        SetupCampaignButtons();
        SetupCustomLevelButtons();
    }

    void SetupCampaignButtons()
    {
        int maxLevelReached = PlayerPrefs.GetInt(MaxLevelReachedKey, 0);

        if (campaignLevel1Button == null)
        {
            Debug.LogError("campaignLevel1Button is not assigned in the Inspector.");
            return;
        }
        campaignLevel1Button.onClick.AddListener(() => LoadCampaignLevel(CampaignLevel1Name));

        if (campaignLevel2Button == null)
        {
            Debug.LogError("campaignLevel2Button is not assigned in the Inspector.");
        }
        else
        {
            if (maxLevelReached >= 1)
            {
                campaignLevel2Button.interactable = true;
                campaignLevel2Button.onClick.AddListener(() => LoadCampaignLevel(CampaignLevel2Name));
            }
            else
            {
                campaignLevel2Button.interactable = false;
            }
        }

        if (campaignLevel3Button == null)
        {
            Debug.LogError("campaignLevel3Button is not assigned in the Inspector.");
        }
        else
        {
            if (maxLevelReached >= 2)
            {
                campaignLevel3Button.interactable = true;
                campaignLevel3Button.onClick.AddListener(() => LoadCampaignLevel(CampaignLevel3Name));
            }
            else
            {
                campaignLevel3Button.interactable = false;
            }
        }
    }

    void SetupCustomLevelButtons()
    {
        string slot1Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot1Name);
        string slot2Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot2Name);
        string slot3Path = Path.Combine(Application.persistentDataPath, CustomLevelSlot3Name);

        if (customLevelSlot1Button == null)
        {
            Debug.LogError("customLevelSlot1Button is not assigned in the Inspector.");
        }
        else
        {
            if (File.Exists(slot1Path))
            {
                customLevelSlot1Button.interactable = true;
                customLevelSlot1Button.onClick.AddListener(() => LoadCustomLevel(1));
            }
            else
            {
                customLevelSlot1Button.interactable = false;
            }
        }

        if (customLevelSlot2Button == null)
        {
            Debug.LogError("customLevelSlot2Button is not assigned in the Inspector.");
        }
        else
        {
            if (File.Exists(slot2Path))
            {
                customLevelSlot2Button.interactable = true;
                customLevelSlot2Button.onClick.AddListener(() => LoadCustomLevel(2));
            }
            else
            {
                customLevelSlot2Button.interactable = false;
            }
        }

        if (customLevelSlot3Button == null)
        {
            Debug.LogError("customLevelSlot3Button is not assigned in the Inspector.");
        }
        else
        {
            if (File.Exists(slot3Path))
            {
                customLevelSlot3Button.interactable = true;
                customLevelSlot3Button.onClick.AddListener(() => LoadCustomLevel(3));
            }
            else
            {
                customLevelSlot3Button.interactable = false;
            }
        }
    }

    void LoadCampaignLevel(string levelName)
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