using UnityEngine;

public class EditorPlacementPreviewHandler : MonoBehaviour
{
    private LevelEditor levelEditor;
    private Camera editorCamera;
    private Collider editorPlaneCollider;

    private float gridSpacing;
    private float previewHeightOffset;

    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    private GameObject placementPreviewInstance;

    public void Init(LevelEditor editor, Camera camera, Collider planeCollider, float spacing, float heightOffset)
    {
        levelEditor = editor;
        editorCamera = camera;
        editorPlaneCollider = planeCollider;
        gridSpacing = spacing;
        previewHeightOffset = heightOffset;
    }

    public void UpdatePreview(int objectType, Quaternion currentRotation, GameObject selectedRandomAsteroidPrefab, Vector3 mousePosition)
    {
        if (levelEditor == null || editorCamera == null || editorPlaneCollider == null)
        {
            DestroyPreview();
            return;
        }

        Ray ray = editorCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (editorPlaneCollider.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector3 previewPosition = hit.point;

            if (gridSpacing > 0)
            {
                previewPosition.x = Mathf.Round(previewPosition.x / gridSpacing) * gridSpacing;
                previewPosition.z = Mathf.Round(previewPosition.z / gridSpacing) * gridSpacing;
            }

            previewPosition.y = editorPlaneCollider.bounds.max.y + previewHeightOffset;

            GameObject prefabToSpawn = (objectType == 2) ? selectedRandomAsteroidPrefab : levelEditor.GetPrefabByType(objectType);

            if (prefabToSpawn != null)
            {
                if (placementPreviewInstance != null && (prefabToSpawn.name + "(Clone)" != placementPreviewInstance.name))
                {
                    DestroyPreview();
                }

                if (placementPreviewInstance == null)
                {
                    placementPreviewInstance = Instantiate(prefabToSpawn);
                    if (Application.isEditor)
                    {
                        placementPreviewInstance.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
                    }

                    Collider[] colliders = placementPreviewInstance.GetComponentsInChildren<Collider>();
                    foreach (Collider col in colliders)
                    {
                        col.enabled = false;
                    }

                    Rigidbody[] rigidbodies = placementPreviewInstance.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rb in rigidbodies)
                    {
                        rb.isKinematic = true;
                        rb.detectCollisions = false;
                    }
                }

                placementPreviewInstance.transform.position = previewPosition;
                placementPreviewInstance.transform.rotation = currentRotation;

                bool canPlace = levelEditor.CheckIfCanPlace(previewPosition, prefabToSpawn, placementPreviewInstance);

                ApplyColorToRenderers(placementPreviewInstance, canPlace ? validPlacementColor : invalidPlacementColor);

                ObjectRadius previewObjectRadius = placementPreviewInstance.GetComponent<ObjectRadius>();
                if (previewObjectRadius != null)
                {
                    previewObjectRadius.ShowRadiusVisualization(!canPlace);
                }

            }
            else
            {
                DestroyPreview();
            }
        }
        else
        {
            DestroyPreview();
        }
    }

    private void ApplyColorToRenderers(GameObject targetObject, Color color)
    {
        MeshRenderer[] meshRenderers = targetObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers)
        {
            if (mr.material != null)
            {
                mr.material.color = color;
            }
        }

        SpriteRenderer[] spriteRenderers = targetObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.color = color;
        }
    }

    public void DestroyPreview()
    {
        if (placementPreviewInstance != null)
        {
            ObjectRadius previewObjectRadius = placementPreviewInstance.GetComponent<ObjectRadius>();
            if (previewObjectRadius != null)
            {
                previewObjectRadius.HideRadiusVisualization();
            }
            DestroyImmediate(placementPreviewInstance);
            placementPreviewInstance = null;
        }
    }
}