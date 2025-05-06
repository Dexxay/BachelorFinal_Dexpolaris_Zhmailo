using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EditorUIHandler : MonoBehaviour
{
    [Header("UI Меню Паузи")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    private LevelEditor levelEditor;

    private Coroutine clearErrorCoroutine;

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Не призначено pauseMenuUI в EditorUIHandler!");
        }

        if (timeLimitSlider != null)
        {
            timeLimitSlider.minValue = 30f;
            timeLimitSlider.maxValue = 120f;
            timeLimitSlider.value = levelEditor.GetTimeLimit();
            UpdateTimeLimitText(levelEditor.GetTimeLimit());
            timeLimitSlider.onValueChanged.AddListener(levelEditor.SetTimeLimit);
        }
        else
        {
            Debug.LogError("Не призначено timeLimitSlider в EditorUIHandler!");
        }


        if (timeLimitText == null)
        {
            Debug.LogError("Не призначено timeLimitText в EditorUIHandler!");
        }

        if (errorText != null)
        {
            errorText.text = "";
        }
        else
        {
            Debug.LogError("Не призначено errorText в EditorUIHandler!");
        }

    }

    public void TogglePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            bool isOpening = !pauseMenuUI.activeSelf;
            pauseMenuUI.SetActive(isOpening);
        }
    }

    public bool IsPauseMenuOpen()
    {
        return pauseMenuUI != null && pauseMenuUI.activeSelf;
    }

    public void UpdateTimeLimitText(float time)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "Час: " + time.ToString("F0") + " сек.";
        }
    }

    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            if (clearErrorCoroutine != null)
            {
                StopCoroutine(clearErrorCoroutine);
            }
            clearErrorCoroutine = StartCoroutine(ClearErrorAfterDelay(3f));
        }
        Debug.Log(message);
    }

    public void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
        if (clearErrorCoroutine != null)
        {
            StopCoroutine(clearErrorCoroutine);
            clearErrorCoroutine = null;
        }
    }

    private IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (errorText != null)
        {
            errorText.text = "";
        }
        clearErrorCoroutine = null;
    }
}