using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class EditorUIHandler : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    [Header("Confirmation Dialog UI")]
    public GameObject confirmationDialogPanel;
    public TextMeshProUGUI confirmationMessageText;
    public Button yesButton;
    public Button noButton;

    [Header("Editor UI Elements")]
    public TextMeshProUGUI selectedElementText;

    private LevelEditor levelEditor;
    private Coroutine clearMessageCoroutine;
    private Action onConfirmAction;

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("pauseMenuUI not assigned in EditorUIHandler!");
        }

        if (timeLimitSlider != null)
        {
            timeLimitSlider.minValue = levelEditor.minSliderTimeLimit;
            timeLimitSlider.maxValue = levelEditor.maxSliderTimeLimit;
            timeLimitSlider.onValueChanged.AddListener(SetTimeLimitFromSlider);
        }
        else
        {
            Debug.LogError("timeLimitSlider not assigned in EditorUIHandler!");
        }


        if (timeLimitText == null)
        {
            Debug.LogError("timeLimitText not assigned in EditorUIHandler!");
        }

        if (errorText == null)
        {
            Debug.LogError("errorText not assigned in EditorUIHandler!");
        }

        if (confirmationDialogPanel != null)
        {
            confirmationDialogPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("confirmationDialogPanel not assigned in EditorUIHandler!");
        }

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }
        else
        {
            Debug.LogError("yesButton not assigned in EditorUIHandler!");
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }
        else
        {
            Debug.LogError("noButton not assigned in EditorUIHandler!");
        }
        if (levelEditor != null)
        {
            UpdateTimeLimitText(levelEditor.GetTimeLimit());
            if (timeLimitSlider != null) timeLimitSlider.value = levelEditor.GetTimeLimit();
        }

        if (selectedElementText == null)
        {
            Debug.LogError("selectedElementText not assigned in EditorUIHandler!");
        }
    }

    public void UpdateSelectedElementDisplay(string elementName)
    {
        if (selectedElementText != null)
        {
            selectedElementText.text = elementName;
        }
    }

    private void SetTimeLimitFromSlider(float value)
    {
        if (levelEditor != null)
        {
            levelEditor.SetTimeLimit(value);
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            bool isActive = !pauseMenuUI.activeSelf;
            pauseMenuUI.SetActive(isActive);
            Time.timeScale = isActive ? 0f : 1f;

            if (isActive && levelEditor != null)
            {
                UpdateTimeLimitText(levelEditor.GetTimeLimit());
                if (timeLimitSlider != null) timeLimitSlider.value = levelEditor.GetTimeLimit();
            }
        }
    }

    public bool IsPauseMenuOpen()
    {
        return pauseMenuUI != null && pauseMenuUI.activeSelf;
    }

    public bool IsConfirmationDialogOpen()
    {
        return confirmationDialogPanel != null && confirmationDialogPanel.activeSelf;
    }

    public void UpdateTimeLimitText(float time)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "Time: " + time.ToString("F0") + " sec.";
        }
    }

    public void SetRedColor()
    {
        if (errorText != null) errorText.color = Color.red;
    }

    public void SetGreenColor()
    {
        if (errorText != null) errorText.color = Color.green;
    }

    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            if (clearMessageCoroutine != null)
            {
                StopCoroutine(clearMessageCoroutine);
            }
            clearMessageCoroutine = StartCoroutine(ClearErrorAfterDelay(3f));
        }
    }

    public void ClearMessage()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
        if (clearMessageCoroutine != null)
        {
            StopCoroutine(clearMessageCoroutine);
            clearMessageCoroutine = null;
        }
    }

    private IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ClearMessage();
    }

    public void ShowConfirmationDialog(string message, Action confirmAction)
    {
        if (confirmationDialogPanel != null && confirmationMessageText != null)
        {
            confirmationMessageText.text = message;
            onConfirmAction = confirmAction;
            confirmationDialogPanel.SetActive(true);
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("Confirmation dialog components are not configured!");
        }
    }

    public void HideConfirmationDialog()
    {
        if (confirmationDialogPanel != null)
        {
            confirmationDialogPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void OnYesClicked()
    {
        onConfirmAction?.Invoke();
        HideConfirmationDialog();
    }

    public void OnNoClicked()
    {
        onConfirmAction = null;
        HideConfirmationDialog();
        if (pauseMenuUI != null && !pauseMenuUI.activeSelf)
        {
            Time.timeScale = 1f;
        }
    }

    public void MainMenuButton_Clicked()
    {
        ShowConfirmationDialog("Return to main menu?\nUnsaved changes will be lost.",
            () => {
                if (levelEditor != null) levelEditor.GoToMainMenuConfirmed();
            });
    }

    public void SaveLevelButton_Clicked(int slot)
    {
        ShowConfirmationDialog($"Save level to slot {slot}?",
            () => {
                if (levelEditor != null) levelEditor.SaveLevelConfirmed(slot);
            });
    }

    public void LoadLevelButton_Clicked(int slot)
    {
        ShowConfirmationDialog($"Load level from slot {slot}?\nUnsaved changes in the current level will be lost.",
           () => {
               if (levelEditor != null) levelEditor.LoadLevelConfirmed(slot);
           });
    }

    public void DeleteLevelButton_Clicked(int slot)
    {
        ShowConfirmationDialog($"Delete level in slot {slot}?",
            () => {
                if (levelEditor != null) levelEditor.DeleteLevelConfirmed(slot);
            });
    }


    public void ClearLevelButton_Clicked()
    {
        ShowConfirmationDialog("Clear all objects on the level?",
            () => {
                if (levelEditor != null) levelEditor.ClearAllObjectsConfirmed();
            });
    }
}