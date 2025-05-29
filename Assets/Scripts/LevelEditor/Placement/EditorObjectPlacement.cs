using UnityEngine;

public class EditorObjectPlacement : MonoBehaviour
{
    private LevelEditor levelEditor;
    private Camera editorCamera;
    private EditorInputHandler inputHandler;
    private EditorPlacementPreviewHandler previewHandler;
    private EditorObjectDeleter deleter;

    private float gridSpacing;
    private float randomHeightRange;
    private Collider editorPlaneCollider;

    private int currentObjectToPlace = 2;
    private GameObject selectedRandomAsteroidPrefab;
    private Quaternion currentPreviewRotation;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip setSound;
    [SerializeField] private AudioClip deleteSound;

    [Header("Placement Settings")]
    [SerializeField] private float previewHeightOffset = 0.1f;


    public int CurrentObjectToPlace { get { return currentObjectToPlace; } }
    public GameObject SelectedRandomAsteroidPrefab { get { return selectedRandomAsteroidPrefab; } }
    public Quaternion CurrentPreviewRotation { get { return currentPreviewRotation; } }


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
            levelEditor.UiHandler.UpdateSlotHighlight(currentObjectToPlace);
        }
        else
        {
            Debug.LogError("UiHandler in LevelEditor is not assigned.");
        }

        if (currentObjectToPlace == 2)
        {
            selectedRandomAsteroidPrefab = levelEditor.GetRandomAsteroidPrefab();
        }
        currentPreviewRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        inputHandler = GetComponent<EditorInputHandler>();
        previewHandler = GetComponent<EditorPlacementPreviewHandler>();
        deleter = GetComponent<EditorObjectDeleter>();

        if (inputHandler != null) inputHandler.Init(levelEditor, this, deleter, previewHandler);
        else Debug.LogError("EditorInputHandler component not found!");

        if (previewHandler != null) previewHandler.Init(levelEditor, editorCamera, editorPlaneCollider, gridSpacing, previewHeightOffset);
        else Debug.LogError("EditorPlacementPreviewHandler component not found!");

        if (deleter != null) deleter.Init(levelEditor, editorCamera, editorPlaneCollider, levelEditor.LevelObjectsParent.transform, audioSource, deleteSound);
        else Debug.LogError("EditorObjectDeleter component not found!");
    }

    void Update()
    {
        if (inputHandler != null) inputHandler.Update();
    }

    void OnDisable()
    {
        if (previewHandler != null) previewHandler.DestroyPreview();
    }

    void OnDestroy()
    {
        if (previewHandler != null) previewHandler.DestroyPreview();
    }


    public void HandleObjectSelection()
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
            levelEditor.UiHandler.ClearMessage();
            levelEditor.UiHandler.UpdateSlotHighlight(currentObjectToPlace);

            if (currentObjectToPlace == 2)
            {
                selectedRandomAsteroidPrefab = levelEditor.GetRandomAsteroidPrefab();
                currentPreviewRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }
            else
            {
                selectedRandomAsteroidPrefab = null;
                currentPreviewRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }


            if (levelEditor.UiHandler != null)
            {
                string displayString = GetElementDisplayName(currentObjectToPlace);
                levelEditor.UiHandler.UpdateSelectedElementDisplay(displayString);
            }
        }
    }

    public void AttemptPlacement(Vector3 mousePosition)
    {
        if (levelEditor == null || editorCamera == null || editorPlaneCollider == null) return;

        Ray ray = editorCamera.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        RaycastHit planeHit = new RaycastHit();
        bool hitPlane = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == editorPlaneCollider)
            {
                planeHit = hit;
                hitPlane = true;
                break;
            }
        }

        if (hitPlane)
        {
            Vector3 placementPosition = planeHit.point;

            if (gridSpacing > 0)
            {
                placementPosition.x = Mathf.Round(placementPosition.x / gridSpacing) * gridSpacing;
                placementPosition.z = Mathf.Round(placementPosition.z / gridSpacing) * gridSpacing;
            }

            GameObject prefabToSpawn = (currentObjectToPlace == 2) ? selectedRandomAsteroidPrefab : levelEditor.GetPrefabByType(currentObjectToPlace);

            if (prefabToSpawn != null)
            {
                Vector3 finalPlacementPosition = placementPosition;
                if (currentObjectToPlace == 2)
                {
                    finalPlacementPosition.y += Random.Range(0f, randomHeightRange);
                }
                else
                {
                    finalPlacementPosition.y = editorPlaneCollider.bounds.max.y + previewHeightOffset;
                }


                bool canPlace = levelEditor.CheckIfCanPlace(finalPlacementPosition, prefabToSpawn, null);


                if (canPlace)
                {
                    Quaternion objectRotation = currentPreviewRotation;

                    GameObject newObject = Instantiate(prefabToSpawn, finalPlacementPosition, objectRotation, levelEditor.LevelObjectsParent.transform);

                    Rigidbody[] rigidbodies = newObject.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in rigidbodies)
                    {
                        rb.isKinematic = true;
                        rb.detectCollisions = false;
                    }

                    PlacedObjectInfo objectInfo = newObject.GetComponent<PlacedObjectInfo>();
                    if (objectInfo == null) objectInfo = newObject.AddComponent<PlacedObjectInfo>();
                    objectInfo.originalPrefabName = prefabToSpawn.name;

                    currentPreviewRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    ObjectRadius placedObjectRadius = newObject.GetComponent<ObjectRadius>();
                    if (placedObjectRadius != null)
                    {
                        placedObjectRadius.CheckAndShowRadiusVisualization();
                    }

                    if (audioSource != null && setSound != null) audioSource.PlayOneShot(setSound);

                    if (currentObjectToPlace == 1)
                    {
                        levelEditor.startAsteroidInstance = newObject;
                        levelEditor.UiHandler.ClearMessage();
                        return;
                    }

                    if (currentObjectToPlace == 0)
                    {
                        levelEditor.finishAsteroidInstance = newObject;
                        levelEditor.UiHandler.ClearMessage();
                        return;
                    }

                    if (currentObjectToPlace == 2)
                    {
                        selectedRandomAsteroidPrefab = levelEditor.GetRandomAsteroidPrefab();
                    }

                }
                else
                {

                }
            }
            else
            {
                levelEditor.ShowMessage("Selected prefab not found or not specified.", true);
            }
        }
    }


    private string GetElementDisplayName(int objectType)
    {
        switch (objectType)
        {
            case 1: return "Start Point";
            case 2: return "Random Asteroid";
            case 3: return "Plasma Gun";
            case 4: return "Laser Gun";
            case 5: return "Laser Ammo";
            case 6: return "Plasma Ammo";
            case 7: return "Health Pack";
            case 8: return "Enemy Turret";
            case 9: return "Enemy UFO Spawner";
            case 0: return "Finish Point";
            default: return "Unknown Element (" + objectType + ")";
        }
    }
}