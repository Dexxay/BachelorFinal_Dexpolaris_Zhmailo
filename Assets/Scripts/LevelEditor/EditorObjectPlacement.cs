using UnityEngine;
using System.Collections.Generic;

public class EditorObjectPlacement : MonoBehaviour
{
    private LevelEditor levelEditor;
    private Camera editorCamera;

    private float gridSpacing;
    private float randomHeightRange;
    private Collider editorPlaneCollider;

    private int currentObjectToPlace = 2;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip setSound;
    [SerializeField] private AudioClip deleteSound;

    public void Init(LevelEditor editor, Camera camera)
    {
        levelEditor = editor;
        editorCamera = camera;

        if (levelEditor == null)
        {
            Debug.LogError("EditorObjectPlacement did not receive a reference to LevelEditor!");
            enabled = false;
            return;
        }
        if (editorCamera == null)
        {
            Debug.LogError("EditorObjectPlacement did not receive a reference to Camera!");
            enabled = false;
            return;
        }

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
    }

    private void HandleObjectPlacementAndDeletion()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider == editorPlaneCollider && Input.GetMouseButtonDown(0))
                {
                    Vector3 placementPosition = hit.point;
                    placementPosition.y += Random.Range(0f, randomHeightRange);

                    if (gridSpacing > 0)
                    {
                        placementPosition.x = Mathf.Round(placementPosition.x / gridSpacing) * gridSpacing;
                        placementPosition.z = Mathf.Round(placementPosition.z / gridSpacing) * gridSpacing;
                    }

                    GameObject prefabToSpawn = levelEditor.GetPrefabByType(currentObjectToPlace);

                    if (prefabToSpawn != null)
                    {
                        if (currentObjectToPlace == 1 && levelEditor.startAsteroidInstance != null)
                        {
                            levelEditor.ShowMessage("Start asteroid already placed. Delete the old one to place a new one.", true);
                            return;
                        }
                        if (currentObjectToPlace == 0 && levelEditor.finishAsteroidInstance != null)
                        {
                            levelEditor.ShowMessage("Finish asteroid already placed. Delete the old one to place a new one.", true);
                            return;
                        }

                        bool canPlace = true;
                        ObjectRadius newObjectRadiusComponent = prefabToSpawn.GetComponent<ObjectRadius>();
                        float newObjectRadius = newObjectRadiusComponent != null ? newObjectRadiusComponent.radius : 1f;

                        foreach (Transform child in levelEditor.levelObjectsParent.transform)
                        {
                            ObjectRadius existingObjectRadiusComponent = child.GetComponent<ObjectRadius>();
                            if (existingObjectRadiusComponent != null)
                            {
                                float distance = Vector3.Distance(placementPosition, child.position);
                                if (distance < newObjectRadius + existingObjectRadiusComponent.radius)
                                {
                                    canPlace = false;
                                    levelEditor.ShowMessage("Objects are overlapping.", true);
                                    break;
                                }
                            }
                        }

                        if (canPlace)
                        {
                            float randomRotationY = Random.Range(0f, 360f);
                            Quaternion objectRotation = Quaternion.Euler(0f, randomRotationY, 0f);

                            GameObject newObject = Instantiate(prefabToSpawn, placementPosition, objectRotation, levelEditor.levelObjectsParent.transform);

                            PlacedObjectInfo objectInfo = newObject.GetComponent<PlacedObjectInfo>();
                            if (objectInfo == null) objectInfo = newObject.AddComponent<PlacedObjectInfo>();
                            objectInfo.originalPrefabName = prefabToSpawn.name;

                            if (currentObjectToPlace == 1) levelEditor.startAsteroidInstance = newObject;
                            if (currentObjectToPlace == 0) levelEditor.finishAsteroidInstance = newObject;

                            audioSource.PlayOneShot(setSound);

                            levelEditor.ClearMessage();
                        }
                    }
                    else
                    {
                        levelEditor.ShowMessage("Selected prefab not found or not specified.", true);
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    PlacedObjectInfo objectToRemoveInfo = hit.collider.GetComponentInParent<PlacedObjectInfo>();
                    if (objectToRemoveInfo != null && objectToRemoveInfo.transform.parent == levelEditor.levelObjectsParent.transform)
                    {
                        if (objectToRemoveInfo.gameObject == levelEditor.startAsteroidInstance)
                        {
                            levelEditor.startAsteroidInstance = null;
                        }
                        if (objectToRemoveInfo.gameObject == levelEditor.finishAsteroidInstance)
                        {
                            levelEditor.finishAsteroidInstance = null;
                        }
                        Destroy(objectToRemoveInfo.gameObject);
                        audioSource.PlayOneShot(deleteSound);
                        levelEditor.ShowMessage("Object deleted.", false);
                    }
                }
            }
        }
    }
}