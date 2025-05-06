using UnityEngine;
using System.Collections.Generic; // Потрібно для списку Prefabs

public class EditorObjectPlacement : MonoBehaviour
{
    // Посилання на головний LevelEditor для доступу до префабів, батьківського об'єкта, помилок тощо.
    private LevelEditor levelEditor;
    private Camera editorCamera;

    // Налаштування розміщення (передаються з LevelEditor)
    private float gridSpacing;
    private float randomHeightRange;
    private Collider editorPlaneCollider;

    private int currentObjectToPlace = 2; // Типовий об'єкт для розміщення

    public void Init(LevelEditor editor, Camera camera)
    {
        levelEditor = editor;
        editorCamera = camera;

        if (levelEditor == null)
        {
            Debug.LogError("EditorObjectPlacement не отримав посилання на LevelEditor!");
            enabled = false; // Вимикаємо скрипт
            return;
        }
        if (editorCamera == null)
        {
            Debug.LogError("EditorObjectPlacement не отримав посилання на Camera!");
            enabled = false;
            return;
        }

        // Отримуємо налаштування з головного LevelEditor
        gridSpacing = levelEditor.gridSpacing;
        randomHeightRange = levelEditor.randomHeightRange;
        editorPlaneCollider = levelEditor.editorPlaneCollider;
    }

    public void HandleInput()
    {
        HandleObjectSelection();
        HandleObjectPlacementAndDeletion();
    }

    private void HandleObjectSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentObjectToPlace = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) currentObjectToPlace = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) currentObjectToPlace = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) currentObjectToPlace = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) currentObjectToPlace = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) currentObjectToPlace = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) currentObjectToPlace = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha8)) currentObjectToPlace = 8;
        else if (Input.GetKeyDown(KeyCode.Alpha9)) currentObjectToPlace = 9;
        else if (Input.GetKeyDown(KeyCode.Alpha0)) currentObjectToPlace = 0;

        // Можна додати відображення поточного обраного об'єкта в UI, якщо потрібно
    }

    private void HandleObjectPlacementAndDeletion()
    {
        // Видалення об'єкта (права кнопка миші)
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Перевіряємо, чи об'єкт, в який влучив промінь, є дочірнім до levelObjectsParent
                if (hit.collider != null && hit.collider.transform.IsChildOf(levelEditor.levelObjectsParent.transform))
                {
                    Transform hitTransform = hit.collider.transform;
                    // Знаходимо кореневий батьківський об'єкт серед дітей levelObjectsParent
                    Transform rootParent = hitTransform;
                    while (rootParent.parent != levelEditor.levelObjectsParent.transform && rootParent.parent != null)
                    {
                        rootParent = rootParent.parent;
                    }
                    GameObject objectToDelete = rootParent.gameObject;


                    // Перевіряємо, чи не видаляємо стартовий або фінішний астероїд
                    if (objectToDelete == levelEditor.startAsteroidInstance)
                    {
                        levelEditor.startAsteroidInstance = null;
                    }
                    else if (objectToDelete == levelEditor.finishAsteroidInstance)
                    {
                        levelEditor.finishAsteroidInstance = null;
                    }

                    Destroy(objectToDelete);
                    levelEditor.ClearError(); // Очищаємо помилку після успішного видалення
                }
            }
        }

        // Розміщення об'єкта (ліва кнопка миші)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Перевіряємо, чи влучили в площину редактора
                if (hit.collider != null && (editorPlaneCollider == hit.collider || hit.collider.CompareTag("EditorPlane")))
                {
                    Vector3 placementPosition = hit.point;
                    // Прив'язка до сітки
                    placementPosition.x = Mathf.Round(placementPosition.x / gridSpacing) * gridSpacing;
                    placementPosition.z = Mathf.Round(placementPosition.z / gridSpacing) * gridSpacing;
                    // Додаємо випадкову висоту
                    placementPosition.y = hit.point.y + Random.Range(0f, randomHeightRange);

                    // Отримуємо префаб для спавну з головного редактора
                    GameObject prefabToSpawn = levelEditor.GetPrefabToSpawn(currentObjectToPlace);

                    if (prefabToSpawn != null)
                    {
                        // Спеціальна перевірка на стартовий/фінішний астероїди (може бути лише один)
                        if (currentObjectToPlace == 1 && levelEditor.startAsteroidInstance != null)
                        {
                            levelEditor.ShowError("Стартовий астероїд вже розміщено.");
                            return;
                        }
                        if (currentObjectToPlace == 0 && levelEditor.finishAsteroidInstance != null)
                        {
                            levelEditor.ShowError("Фінішний астероїд вже розміщено.");
                            return;
                        }

                        // Перевірка на перетин з іншими об'єктами (по радіусу)
                        bool canPlace = true;
                        // Припускаємо, що у префабів є компонент ObjectRadius з полем radius
                        ObjectRadius newObjectRadiusComponent = prefabToSpawn.GetComponent<ObjectRadius>();
                        if (newObjectRadiusComponent != null)
                        {
                            float newRadius = newObjectRadiusComponent.radius;
                            // Ітеруємо по всіх об'єктах, які вже розміщені під батьківським об'єктом
                            foreach (Transform child in levelEditor.levelObjectsParent.transform)
                            {
                                ObjectRadius existingObjectRadiusComponent = child.GetComponent<ObjectRadius>();
                                if (existingObjectRadiusComponent != null)
                                {
                                    float existingRadius = existingObjectRadiusComponent.radius;
                                    Vector2 posNew = new Vector2(placementPosition.x, placementPosition.z);
                                    Vector2 posExisting = new Vector2(child.position.x, child.position.z);
                                    // Перевірка дистанції на XZ площині
                                    if (Vector2.Distance(posNew, posExisting) < newRadius + existingRadius)
                                    {
                                        canPlace = false;
                                        levelEditor.ShowError("Об'єкти перетинаються.");
                                        break; // Зупиняємо перевірку, якщо знайдено перетин
                                    }
                                }
                            }
                        }
                        // Якщо префаб не має компонента ObjectRadius, вважаємо, що перевірка не потрібна або реалізована інакше

                        if (canPlace)
                        {
                            // Випадковий поворот по осі Y
                            float randomRotationY = Random.Range(0f, 360f);
                            Quaternion objectRotation = Quaternion.Euler(0f, randomRotationY, 0f);

                            // Створюємо екземпляр об'єкта як дочірній до levelObjectsParent
                            GameObject newObject = Instantiate(prefabToSpawn, placementPosition, objectRotation, levelEditor.levelObjectsParent.transform);

                            // Оновлюємо посилання на стартовий/фінішний астероїди
                            if (currentObjectToPlace == 1) levelEditor.startAsteroidInstance = newObject;
                            if (currentObjectToPlace == 0) levelEditor.finishAsteroidInstance = newObject;

                            levelEditor.ClearError(); // Очищаємо помилку після успішного розміщення
                        }
                    }
                    else
                    {
                        levelEditor.ShowError("Обраний префаб не знайдено або він не вказаний.");
                    }
                }
            }
        }
    }
}