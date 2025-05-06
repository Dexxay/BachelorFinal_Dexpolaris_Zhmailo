using UnityEngine;
using System.Collections.Generic;
using System.IO; // Потрібно для роботи з файлами
using System.Runtime.Serialization.Formatters.Binary; // Потрібно для серіалізації/десеріалізації

// Класи для даних рівня (залишаються Serializable)
[System.Serializable]
public class ObjectData
{
    public string prefabName;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class LevelData
{
    public float timeLimit;
    public List<ObjectData> placedObjects;
}


public class LevelSaveLoadManager : MonoBehaviour
{
    private LevelEditor levelEditor; // Посилання на головний LevelEditor

    public void Init(LevelEditor editor)
    {
        levelEditor = editor;
        if (levelEditor == null)
        {
            Debug.LogError("LevelSaveLoadManager не отримав посилання на LevelEditor!");
            enabled = false;
        }
    }

    // Збереження рівня у вказаний слот
    public void SaveLevel(int slot)
    {
        // Перевірка на наявність стартового та фінішного астероїдів перед збереженням
        if (levelEditor.startAsteroidInstance == null || levelEditor.finishAsteroidInstance == null)
        {
            levelEditor.ShowError("На рівні повинні бути присутні стартовий та фінішний астероїди для збереження.");
            return;
        }
        levelEditor.ClearError(); // Очищаємо попередні помилки, якщо збереження успішне

        // Отримуємо шлях до файлу збереження
        string filePath = levelEditor.GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return; // GetSaveFilePath вже показав помилку

        // Створюємо об'єкт для збереження даних рівня
        LevelData levelData = new LevelData();
        levelData.timeLimit = levelEditor.GetTimeLimit(); // Отримуємо ліміт часу з LevelEditor
        levelData.placedObjects = new List<ObjectData>();

        // Збираємо дані про всі розміщені об'єкти
        if (levelEditor.levelObjectsParent != null)
        {
            foreach (Transform child in levelEditor.levelObjectsParent.transform)
            {
                ObjectData objectData = new ObjectData();
                // Важливо: зберігаємо ім'я префабу без "(Clone)"
                objectData.prefabName = child.gameObject.name.Replace("(Clone)", "").Trim();
                objectData.position = child.position;
                objectData.rotation = child.rotation;
                levelData.placedObjects.Add(objectData);
            }
        }

        // Використовуємо BinaryFormatter для серіалізації
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = null; // Оголошуємо FileStream поза try блоком

        try
        {
            file = File.Create(filePath); // Створюємо файл
            formatter.Serialize(file, levelData); // Серіалізуємо дані
            Debug.Log($"Рівень збережено в слот {slot} за шляхом: {filePath}");
            levelEditor.ShowError($"Рівень збережено в слот {slot}"); // Відображаємо повідомлення в UI
        }
        catch (System.Exception e)
        {
            levelEditor.ShowError($"Помилка збереження рівня у слот {slot}: {e.Message}"); // Відображаємо помилку в UI
            Debug.LogError($"Помилка збереження рівня: {e.ToString()}"); // Виводимо повну помилку в консоль
        }
        finally
        {
            // Закриваємо файл, якщо він був відкритий
            if (file != null) file.Close();
        }
    }

    // Завантаження рівня зі вказаного слоту
    public void LoadLevel(int slot)
    {
        string filePath = levelEditor.GetSaveFilePath(slot);
        if (string.IsNullOrEmpty(filePath)) return; // GetSaveFilePath вже показав помилку


        // Перевіряємо, чи існує файл збереження
        if (!File.Exists(filePath))
        {
            levelEditor.ShowError($"Збереження у слоті {slot} не знайдено.");
            Debug.LogWarning($"Файл збереження не знайдено за шляхом: {filePath}");
            return;
        }

        // Очищаємо поточний рівень перед завантаженням
        levelEditor.ClearLevel();

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = null;
        LevelData loadedData = null; // Змінна для десеріалізованих даних

        try
        {
            file = File.Open(filePath, FileMode.Open); // Відкриваємо файл
            loadedData = (LevelData)formatter.Deserialize(file); // Десеріалізуємо дані
        }
        catch (System.Exception e)
        {
            levelEditor.ShowError($"Помилка завантаження рівня зі слоту {slot}: {e.Message}"); // Відображаємо помилку в UI
            Debug.LogError($"Помилка завантаження рівня: {e.ToString()}"); // Виводимо повну помилку в консоль
                                                                           // Гарантуємо закриття файлу навіть при помилці
        }
        finally
        {
            if (file != null) file.Close();
        }


        // Перевіряємо, чи вдалося десеріалізувати дані
        if (loadedData == null)
        {
            levelEditor.ShowError($"Не вдалося десеріалізувати дані рівня зі слоту {slot}. Файл може бути пошкоджений.");
            return;
        }


        // Встановлюємо завантажений ліміт часу
        levelEditor.SetTimeLimit(loadedData.timeLimit);
        // Оновлюємо повзунок та текст через UIHandler, який викликається SetTimeLimit

        // Відновлюємо розміщені об'єкти
        if (levelEditor.levelObjectsParent == null)
        {
            Debug.LogError("Батьківський об'єкт для рівня LevelObjects не знайдено під час завантаження!");
            levelEditor.ShowError("Помилка завантаження: Не знайдено батьківського об'єкта для об'єктів рівня.");
            return;
        }

        // Скидаємо посилання на стартовий/фінішний перед завантаженням, ClearLevel вже робить це
        // levelEditor.startAsteroidInstance = null;
        // levelEditor.finishAsteroidInstance = null;

        foreach (ObjectData objectData in loadedData.placedObjects)
        {
            // Знаходимо відповідний префаб за іменем
            GameObject prefabToInstantiate = levelEditor.GetPrefabByName(objectData.prefabName);

            if (prefabToInstantiate != null)
            {
                // Створюємо екземпляр об'єкта
                GameObject instantiatedObject = Instantiate(prefabToInstantiate, objectData.position, objectData.rotation, levelEditor.levelObjectsParent.transform);

                // Оновлюємо посилання на стартовий/фінішний астероїди
                if (levelEditor.startAsteroidPrefab != null && prefabToInstantiate.name == levelEditor.startAsteroidPrefab.name)
                {
                    levelEditor.startAsteroidInstance = instantiatedObject;
                }
                else if (levelEditor.finishAsteroidPrefab != null && prefabToInstantiate.name == levelEditor.finishAsteroidPrefab.name)
                {
                    levelEditor.finishAsteroidInstance = instantiatedObject;
                }
            }
            else
            {
                Debug.LogWarning($"Префаб з ім'ям '{objectData.prefabName}' не знайдено під час завантаження!");
                // Можливо, варто показати повідомлення користувачеві про пропущені об'єкти
            }
        }

        Debug.Log($"Рівень завантажено зі слоту {slot}");
        levelEditor.ShowError($"Рівень завантажено зі слоту {slot}"); // Відображаємо повідомлення в UI

        // Можна закрити меню паузи після завантаження, якщо воно було відкрите
        // uiHandler.TogglePauseMenu(); // Потрібно посилання на UIHandler, або викликати з LevelEditor
        // Альтернативно, це може бути зроблено UI кнопкою "Close Menu"
    }
}