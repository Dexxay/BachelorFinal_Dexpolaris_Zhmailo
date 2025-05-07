using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EditorUIHandler : MonoBehaviour
{
    [Header("UI ���� �����")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    private LevelEditor levelEditor;

    private Coroutine clearMessageCoroutine;

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        else
        {
            Debug.LogError("�� ���������� pauseMenuUI � EditorUIHandler!");
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
            Debug.LogError("�� ���������� timeLimitSlider � EditorUIHandler!");
        }


        if (timeLimitText == null)
        {
            Debug.LogError("�� ���������� timeLimitText � EditorUIHandler!");
        }

        if (errorText != null)
        {
            errorText.text = "";
        }
        else
        {
            Debug.LogError("�� ���������� errorText � EditorUIHandler!");
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
            timeLimitText.text = "���: " + time.ToString("F0") + " ���.";
            timeLimitSlider.value = time;
        }
    }

    public void SetRedColor ()
    {
        errorText.color = Color.red;
    }

    public void SetGreenColor()
    {
        errorText.color = Color.green;
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
        Debug.Log(message);
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
        if (errorText != null)
        {
            errorText.text = "";
        }
        clearMessageCoroutine = null;
    }
}