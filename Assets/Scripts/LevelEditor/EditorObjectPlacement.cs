using UnityEngine;
using System.Collections.Generic; // ������� ��� ������ Prefabs

public class EditorObjectPlacement : MonoBehaviour
{
    // ��������� �� �������� LevelEditor ��� ������� �� �������, ������������ ��'����, ������� ����.
    private LevelEditor levelEditor;
    private Camera editorCamera;

    // ������������ ��������� (����������� � LevelEditor)
    private float gridSpacing;
    private float randomHeightRange;
    private Collider editorPlaneCollider;

    private int currentObjectToPlace = 2; // ������� ��'��� ��� ���������

    public void Init(LevelEditor editor, Camera camera)
    {
        levelEditor = editor;
        editorCamera = camera;

        if (levelEditor == null)
        {
            Debug.LogError("EditorObjectPlacement �� ������� ��������� �� LevelEditor!");
            enabled = false; // �������� ������
            return;
        }
        if (editorCamera == null)
        {
            Debug.LogError("EditorObjectPlacement �� ������� ��������� �� Camera!");
            enabled = false;
            return;
        }

        // �������� ������������ � ��������� LevelEditor
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

        // ����� ������ ����������� ��������� �������� ��'���� � UI, ���� �������
    }

    private void HandleObjectPlacementAndDeletion()
    {
        // ��������� ��'���� (����� ������ ����)
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ����������, �� ��'���, � ���� ������ ������, � ������� �� levelObjectsParent
                if (hit.collider != null && hit.collider.transform.IsChildOf(levelEditor.levelObjectsParent.transform))
                {
                    Transform hitTransform = hit.collider.transform;
                    // ��������� ��������� ����������� ��'��� ����� ���� levelObjectsParent
                    Transform rootParent = hitTransform;
                    while (rootParent.parent != levelEditor.levelObjectsParent.transform && rootParent.parent != null)
                    {
                        rootParent = rootParent.parent;
                    }
                    GameObject objectToDelete = rootParent.gameObject;


                    // ����������, �� �� ��������� ��������� ��� ������� �������
                    if (objectToDelete == levelEditor.startAsteroidInstance)
                    {
                        levelEditor.startAsteroidInstance = null;
                    }
                    else if (objectToDelete == levelEditor.finishAsteroidInstance)
                    {
                        levelEditor.finishAsteroidInstance = null;
                    }

                    Destroy(objectToDelete);
                    levelEditor.ClearError(); // ������� ������� ���� �������� ���������
                }
            }
        }

        // ��������� ��'���� (��� ������ ����)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // ����������, �� ������� � ������� ���������
                if (hit.collider != null && (editorPlaneCollider == hit.collider || hit.collider.CompareTag("EditorPlane")))
                {
                    Vector3 placementPosition = hit.point;
                    // ����'���� �� ����
                    placementPosition.x = Mathf.Round(placementPosition.x / gridSpacing) * gridSpacing;
                    placementPosition.z = Mathf.Round(placementPosition.z / gridSpacing) * gridSpacing;
                    // ������ ��������� ������
                    placementPosition.y = hit.point.y + Random.Range(0f, randomHeightRange);

                    // �������� ������ ��� ������ � ��������� ���������
                    GameObject prefabToSpawn = levelEditor.GetPrefabToSpawn(currentObjectToPlace);

                    if (prefabToSpawn != null)
                    {
                        // ���������� �������� �� ���������/������� �������� (���� ���� ���� ����)
                        if (currentObjectToPlace == 1 && levelEditor.startAsteroidInstance != null)
                        {
                            levelEditor.ShowError("��������� ������� ��� ��������.");
                            return;
                        }
                        if (currentObjectToPlace == 0 && levelEditor.finishAsteroidInstance != null)
                        {
                            levelEditor.ShowError("Գ����� ������� ��� ��������.");
                            return;
                        }

                        // �������� �� ������� � ������ ��'������ (�� ������)
                        bool canPlace = true;
                        // ����������, �� � ������� � ��������� ObjectRadius � ����� radius
                        ObjectRadius newObjectRadiusComponent = prefabToSpawn.GetComponent<ObjectRadius>();
                        if (newObjectRadiusComponent != null)
                        {
                            float newRadius = newObjectRadiusComponent.radius;
                            // ������� �� ��� ��'�����, �� ��� ������� �� ����������� ��'�����
                            foreach (Transform child in levelEditor.levelObjectsParent.transform)
                            {
                                ObjectRadius existingObjectRadiusComponent = child.GetComponent<ObjectRadius>();
                                if (existingObjectRadiusComponent != null)
                                {
                                    float existingRadius = existingObjectRadiusComponent.radius;
                                    Vector2 posNew = new Vector2(placementPosition.x, placementPosition.z);
                                    Vector2 posExisting = new Vector2(child.position.x, child.position.z);
                                    // �������� ��������� �� XZ ������
                                    if (Vector2.Distance(posNew, posExisting) < newRadius + existingRadius)
                                    {
                                        canPlace = false;
                                        levelEditor.ShowError("��'���� �������������.");
                                        break; // ��������� ��������, ���� �������� �������
                                    }
                                }
                            }
                        }
                        // ���� ������ �� �� ���������� ObjectRadius, �������, �� �������� �� ������� ��� ���������� ������

                        if (canPlace)
                        {
                            // ���������� ������� �� �� Y
                            float randomRotationY = Random.Range(0f, 360f);
                            Quaternion objectRotation = Quaternion.Euler(0f, randomRotationY, 0f);

                            // ��������� ��������� ��'���� �� ������� �� levelObjectsParent
                            GameObject newObject = Instantiate(prefabToSpawn, placementPosition, objectRotation, levelEditor.levelObjectsParent.transform);

                            // ��������� ��������� �� ���������/������� ��������
                            if (currentObjectToPlace == 1) levelEditor.startAsteroidInstance = newObject;
                            if (currentObjectToPlace == 0) levelEditor.finishAsteroidInstance = newObject;

                            levelEditor.ClearError(); // ������� ������� ���� �������� ���������
                        }
                    }
                    else
                    {
                        levelEditor.ShowError("������� ������ �� �������� ��� �� �� ��������.");
                    }
                }
            }
        }
    }
}