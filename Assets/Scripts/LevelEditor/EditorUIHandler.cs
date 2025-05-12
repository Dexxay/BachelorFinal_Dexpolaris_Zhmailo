using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;  

public class EditorUIHandler : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Slider timeLimitSlider;
    [SerializeField] private TextMeshProUGUI timeLimitText;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("Confirmation Dialog UI")]
    [SerializeField] private GameObject confirmationDialogPanel;
    [SerializeField] private TextMeshProUGUI confirmationMessageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Editor UI Elements")]
    [SerializeField] private TextMeshProUGUI selectedElementText;
    [SerializeField] private List<Image> slotHighlights;

    private LevelEditor levelEditor;
    private Coroutine clearMessageCoroutine;
    private Action onConfirmAction;

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;
        if (levelEditor == null)
        {
            Debug.LogError("LevelEditor is not assigned to EditorUIHandler.");
            return;
        }

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
            timeLimitSlider.minValue = levelEditor.MinSliderTimeLimit;
            timeLimitSlider.maxValue = levelEditor.MaxSliderTimeLimit;
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

        UpdateTimeLimitText(levelEditor.GetTimeLimit());
        if (timeLimitSlider != null) timeLimitSlider.value = levelEditor.GetTimeLimit();


        if (selectedElementText == null)
        {
            Debug.LogError("selectedElementText not assigned in EditorUIHandler!");
        }

         UpdateSlotHighlight(levelEditor.GetComponent<EditorObjectPlacement>().CurrentObjectToPlace);
    }

    public void UpdateSelectedElementDisplay(string elementName)
    {
        if (selectedElementText != null)
        {
            selectedElementText.text = elementName;
        }
    }

     public void UpdateSlotHighlight(int activeSlotIndex)
    {
        float visibleTransparency = 0.9f;
        float hiddenTransperency = 0.2f;
        for (int i = 0; i < slotHighlights.Count; i++)
        {
            if (slotHighlights[i] != null)
            {
                int correspondingListIndex = (activeSlotIndex == 0) ? 9 : activeSlotIndex - 1;

                Color targetColor = slotHighlights[i].color;

                if (i == correspondingListIndex)
                {
                    targetColor.a = visibleTransparency;
                }
                else
                {
                    targetColor.a = hiddenTransperency;
                }
                slotHighlights[i].color = targetColor;
            }
            else
            {
                Debug.LogWarning($"Slot highlight element at index {i} is null in the list.");
            }
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
        if (pauseMenuUI == null || !pauseMenuUI.activeSelf)
        {
            Time.timeScale = 1f;
        }
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
    }

    public void MainMenuButton_Clicked()
    {
        if (levelEditor == null) return;
        ShowConfirmationDialog("Return to main menu?\nUnsaved changes will be lost.",
            () => {
                levelEditor.GoToMainMenuConfirmed();
            });
    }

    public void SaveLevelButton_Clicked(int slot)
    {
        if (levelEditor == null) return;
        ShowConfirmationDialog($"Save level to slot {slot}?",
            () => {
                levelEditor.SaveLevelConfirmed(slot);
            });
    }

    public void LoadLevelButton_Clicked(int slot)
    {
        if (levelEditor == null) return;
        ShowConfirmationDialog($"Load level from slot {slot}?\nUnsaved changes in the current level will be lost.",
           () => {
               levelEditor.LoadLevelConfirmed(slot);
           });
    }

    public void DeleteLevelButton_Clicked(int slot)
    {
        if (levelEditor == null) return;
        ShowConfirmationDialog($"Delete level in slot {slot}?",
            () => {
                levelEditor.DeleteLevelConfirmed(slot);
            });
    }


    public void ClearLevelButton_Clicked()
    {
        if (levelEditor == null) return;
        ShowConfirmationDialog("Clear all objects on the level?",
            () => {
                levelEditor.ClearAllObjectsConfirmed();
            });
    }
}