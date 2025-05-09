using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
            Debug.LogError("LevelEditor is not assigned to EditorObjectPlacement.");
            enabled = false;
            return;
        }
        if (editorCamera == null)
        {
            Debug.LogError("EditorCamera is not assigned to EditorObjectPlacement.");
            enabled = false;
            return;
        }

        gridSpacing = levelEditor.GridSpacing;
        randomHeightRange = levelEditor.RandomHeightRange;
        editorPlaneCollider = levelEditor.EditorPlaneCollider;

        if (levelEditor.UiHandler != null)
        {
            string displayString = GetElementDisplayName(currentObjectToPlace);
            levelEditor.UiHandler.UpdateSelectedElementDisplay(displayString);
        }
        else
        {
            Debug.LogError("UiHandler in LevelEditor is not assigned.");
        }
    }

    public void HandleInput()
    {
        HandleObjectSelection();
        HandleObjectPlacementAndDeletion();
    }

    private void HandleObjectSelection()
    {
        int previousObjectToPlace = currentObjectToPlace;

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

        if (currentObjectToPlace != previousObjectToPlace)
        {
            if (levelEditor.UiHandler != null)
            {
                string displayString = GetElementDisplayName(currentObjectToPlace);
                levelEditor.UiHandler.UpdateSelectedElementDisplay(displayString);
            }
            else
            {
                Debug.LogError("UiHandler in LevelEditor is not assigned.");
            }
        }
    }

    private string GetElementDisplayName(int objectType)
    {
        switch (objectType)
        {
            case 1:
                return "Start Point";
            case 2:
                return "Random Asteroid";
            case 3:
                return "Plasma Gun";
            case 4:
                return "Laser Gun";
            case 5:
                return "Laser Ammo";
            case 6:
                return "Plasma Ammo";
            case 7:
                return "Health Pack";
            case 8:
                return "Enemy Turret";
            case 9:
                return "Enemy UFO Spawner";
            case 0:
                return "Finish Point";
            default:
                return "Unknown Element (" + objectType + ")";
        }
    }


    private void HandleObjectPlacementAndDeletion()
    {
        if (levelEditor == null) return;


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

                        if (levelEditor.LevelObjectsParent == null)
                        {
                            Debug.LogError("LevelObjectsParent in LevelEditor is not assigned.");
                            return;
                        }

                        foreach (Transform child in levelEditor.LevelObjectsParent.transform)
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

                            GameObject newObject = Instantiate(prefabToSpawn, placementPosition, objectRotation, levelEditor.LevelObjectsParent.transform);

                            PlacedObjectInfo objectInfo = newObject.GetComponent<PlacedObjectInfo>();
                            if (objectInfo == null) objectInfo = newObject.AddComponent<PlacedObjectInfo>();
                            objectInfo.originalPrefabName = prefabToSpawn.name;

                            if (currentObjectToPlace == 1) levelEditor.startAsteroidInstance = newObject;
                            if (currentObjectToPlace == 0) levelEditor.finishAsteroidInstance = newObject;

                            if (audioSource != null && setSound != null) audioSource.PlayOneShot(setSound);

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
                    if (levelEditor.LevelObjectsParent == null)
                    {
                        Debug.LogError("LevelObjectsParent in LevelEditor is not assigned.");
                        return;
                    }
                    if (objectToRemoveInfo != null && objectToRemoveInfo.transform.parent == levelEditor.LevelObjectsParent.transform)
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
                        if (audioSource != null && deleteSound != null) audioSource.PlayOneShot(deleteSound);
                        levelEditor.ShowMessage("Object deleted.", false);
                    }
                }
            }
        }
    }
}