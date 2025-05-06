using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class ControlsMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonJump;
    [SerializeField] private TextMeshProUGUI buttonSprint;
    [SerializeField] private TextMeshProUGUI buttonReload;

    [SerializeField] private KeyCodesDefaults keyCodesDefaults;

    private string jumpKeyString = "JumpKey";
    private string sprintKeyString = "SprintKey";
    private string reloadKeyString = "ReloadKey";

    void Start()
    {
        buttonJump.text = PlayerPrefs.GetString(jumpKeyString, keyCodesDefaults.defaultButtonJump.ToString());
        buttonSprint.text = PlayerPrefs.GetString(sprintKeyString, keyCodesDefaults.defaultButtonSprint.ToString());
        buttonReload.text = PlayerPrefs.GetString(reloadKeyString, keyCodesDefaults.defaultButtonReload.ToString());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }

    public void ChangeKey(TextMeshProUGUI buttonText)
    {
        string startText = buttonText.text;
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(keyCode))
            {
                buttonText.text = keyCode.ToString();
                return;
            }
        }
        buttonText.text = startText;
    }

    public void SaveKeys()
    {
        if (IsThereDublicates())
            SetDefault();

        PlayerPrefs.SetString(jumpKeyString, buttonJump.text);
        PlayerPrefs.SetString(sprintKeyString, buttonSprint.text);
        PlayerPrefs.SetString(reloadKeyString, buttonReload.text);

        PlayerPrefs.Save();
    }

    public bool IsThereDublicates()
    {
        int reservedKeysAmount = 9;
        HashSet<string> keySet = new HashSet<string>();

        keySet.Add(buttonJump.text);
        keySet.Add(buttonSprint.text);
        keySet.Add(buttonReload.text);

        keySet.Add(keyCodesDefaults.defaultForward.ToString());
        keySet.Add(keyCodesDefaults.defaultBackward.ToString());
        keySet.Add(keyCodesDefaults.defaultLeft.ToString());
        keySet.Add(keyCodesDefaults.defaultRight.ToString());

        keySet.Add(keyCodesDefaults.defaultShot.ToString());
        keySet.Add(keyCodesDefaults.defaultEscape.ToString());

        if (keySet.Count != reservedKeysAmount)
            return true;
        return false;
    }

    public void SetDefault()
    {
        buttonJump.text = keyCodesDefaults.defaultButtonJump.ToString();
        buttonSprint.text = keyCodesDefaults.defaultButtonSprint.ToString();
        buttonReload.text = keyCodesDefaults.defaultButtonReload.ToString();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

}
