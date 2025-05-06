using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // ������� ��� Coroutines

public class EditorUIHandler : MonoBehaviour
{
    [Header("UI ���� �����")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    private LevelEditor levelEditor; // ��������� �� �������� LevelEditor

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
            Debug.LogError("�� ���������� pauseMenuUI � EditorUIHandler!");
        }

        if (timeLimitSlider != null)
        {
            timeLimitSlider.minValue = 30f;
            timeLimitSlider.maxValue = 120f;
            // ������������ ��������� �������� � LevelEditor � ���������� �� ����
            timeLimitSlider.value = levelEditor.GetTimeLimit();
            UpdateTimeLimitText(levelEditor.GetTimeLimit()); // ��������� ����� ��� �����
            // ������ Listener ����� ���, ��� ���������� �������� � LevelEditor
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
            errorText.text = ""; // ������� ����� ������� ��� �����
        }
        else
        {
            Debug.LogError("�� ���������� errorText � EditorUIHandler!");
        }

        // ���������� ������� ������� ���� ����� � ��������� Unity:
        // ������ Save Slot 1 -> EditorUIHandler -> levelEditor.SaveLevel(1)
        // ������ Load Slot 1 -> EditorUIHandler -> levelEditor.LoadLevel(1)
        // ������ Clear Level -> EditorUIHandler -> levelEditor.ClearLevel()
        // ������ Resume/Close Menu -> EditorUIHandler -> TogglePauseMenu() (��� ������� � ���������� LevelEditor)
    }

    // ����������� �������� ���� �����
    public void TogglePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            bool isOpening = !pauseMenuUI.activeSelf;
            pauseMenuUI.SetActive(isOpening);
            // ��� ����� ������ ����� �������/���������� ����, ���� levelEditor
            // �� ������� �������� ��� (���� � ��� �� �����������)
        }
    }

    // ��������, �� ���� ����� �������
    public bool IsPauseMenuOpen()
    {
        return pauseMenuUI != null && pauseMenuUI.activeSelf;
    }

    // ��������� ������ �������� ����
    public void UpdateTimeLimitText(float time)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "���: " + time.ToString("F0") + " ���.";
        }
    }

    // ³���������� ������ ������� � ��������
    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            // ��������� ��������� �������, ���� �� ��� ��������
            if (clearErrorCoroutine != null)
            {
                StopCoroutine(clearErrorCoroutine);
            }
            // ��������� ����� ������� ��� �������� ������ ����� ������ ���
            clearErrorCoroutine = StartCoroutine(ClearErrorAfterDelay(3f)); // 3 �������
        }
        Debug.Log(message); // ����� �������� � �������
    }

    // ������� �������� ����� �������
    public void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
        // ��������� �������, ���� �� ��������
        if (clearErrorCoroutine != null)
        {
            StopCoroutine(clearErrorCoroutine);
            clearErrorCoroutine = null;
        }
    }

    // ������� ��� �������� ������ ������� ����� ��������
    private IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // ������������� Realtime, ������� timeScale = 0
        if (errorText != null)
        {
            errorText.text = "";
        }
        clearErrorCoroutine = null; // ������� ��������� ���� ����������
    }
}