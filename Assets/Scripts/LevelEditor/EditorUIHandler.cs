using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Потрібно для Coroutines

public class EditorUIHandler : MonoBehaviour
{
    [Header("UI Меню Паузи")]
    public GameObject pauseMenuUI;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;
    public TextMeshProUGUI errorText;

    private LevelEditor levelEditor; // Посилання на головний LevelEditor

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
            // Встановлюємо початкове значення з LevelEditor і підписуємось на зміни
            timeLimitSlider.value = levelEditor.GetTimeLimit();
            UpdateTimeLimitText(levelEditor.GetTimeLimit()); // Оновлюємо текст при старті
            // Додаємо Listener через код, щоб передавати значення в LevelEditor
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
            errorText.text = ""; // Очищаємо текст помилки при старті
        }
        else
        {
            Debug.LogError("Не призначено errorText в EditorUIHandler!");
        }

        // Призначаємо функції кнопкам меню паузи в Інспекторі Unity:
        // Кнопка Save Slot 1 -> EditorUIHandler -> levelEditor.SaveLevel(1)
        // Кнопка Load Slot 1 -> EditorUIHandler -> levelEditor.LoadLevel(1)
        // Кнопка Clear Level -> EditorUIHandler -> levelEditor.ClearLevel()
        // Кнопка Resume/Close Menu -> EditorUIHandler -> TogglePauseMenu() (або напряму з компонента LevelEditor)
    }

    // Перемикання видимості меню паузи
    public void TogglePauseMenu()
    {
        if (pauseMenuUI != null)
        {
            bool isOpening = !pauseMenuUI.activeSelf;
            pauseMenuUI.SetActive(isOpening);
            // Тут можна додати логіку зупинки/відновлення часу, якщо levelEditor
            // не повністю заморожує час (хоча у вас він заморожений)
        }
    }

    // Перевірка, чи меню паузи відкрите
    public bool IsPauseMenuOpen()
    {
        return pauseMenuUI != null && pauseMenuUI.activeSelf;
    }

    // Оновлення тексту повзунка часу
    public void UpdateTimeLimitText(float time)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "Час: " + time.ToString("F0") + " сек.";
        }
    }

    // Відображення тексту помилки з таймером
    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            // Зупиняємо попередній корутин, якщо він був активний
            if (clearErrorCoroutine != null)
            {
                StopCoroutine(clearErrorCoroutine);
            }
            // Запускаємо новий корутин для очищення тексту через деякий час
            clearErrorCoroutine = StartCoroutine(ClearErrorAfterDelay(3f)); // 3 секунди
        }
        Debug.Log(message); // Також виводимо в консоль
    }

    // Негайно очистити текст помилки
    public void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
        // Зупиняємо корутин, якщо він активний
        if (clearErrorCoroutine != null)
        {
            StopCoroutine(clearErrorCoroutine);
            clearErrorCoroutine = null;
        }
    }

    // Корутин для очищення тексту помилки через затримку
    private IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Використовуємо Realtime, оскільки timeScale = 0
        if (errorText != null)
        {
            errorText.text = "";
        }
        clearErrorCoroutine = null; // Скидаємо посилання після завершення
    }
}