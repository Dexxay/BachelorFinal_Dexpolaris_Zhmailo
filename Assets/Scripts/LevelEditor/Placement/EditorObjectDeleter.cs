using System.Collections.Generic;
using UnityEngine;

public class EditorObjectDeleter : MonoBehaviour
{
    private LevelEditor levelEditor;
    private Camera editorCamera;
    private Collider editorPlaneCollider;
    private Transform levelObjectsParent;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deleteSound;

    public void Init(LevelEditor editor, Camera camera, Collider planeCollider, Transform parentTransform, AudioSource soundSource, AudioClip soundClip)
    {
        levelEditor = editor;
        editorCamera = camera;
        editorPlaneCollider = planeCollider;
        levelObjectsParent = parentTransform;
        audioSource = soundSource;
        deleteSound = soundClip;
    }

    public void AttemptDeletion(Vector3 mousePosition)
    {
        if (levelEditor == null || editorCamera == null || levelObjectsParent == null) return;

        Ray ray = editorCamera.ScreenPointToRay(mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        GameObject objectToDestroy = null;

        foreach (RaycastHit hit in hits)
        {
            PlacedObjectInfo objectToRemoveInfo = hit.collider.GetComponentInParent<PlacedObjectInfo>();

            if (objectToRemoveInfo != null && objectToRemoveInfo.transform.parent == levelObjectsParent)
            {
                objectToDestroy = objectToRemoveInfo.gameObject;
                break;
            }
            if (hit.collider == editorPlaneCollider)
            {

            }
        }

        if (objectToDestroy != null)
        {
            DeleteObject(objectToDestroy);
        }
    }

    private void DeleteObject(GameObject obj)
    {
        if (obj == levelEditor.startAsteroidInstance)
        {
            levelEditor.startAsteroidInstance = null;
        }
        if (obj == levelEditor.finishAsteroidInstance)
        {
            levelEditor.finishAsteroidInstance = null;
        }
        Destroy(obj);
        if (audioSource != null && deleteSound != null) audioSource.PlayOneShot(deleteSound);
    }


    public void DeleteObjectsBelowPlane()
    {
        if (levelEditor == null || levelObjectsParent == null)
        {
            Debug.LogError("LevelEditor or LevelObjectsParent is not assigned. Cannot delete objects.");
            return;
        }

        if (editorPlaneCollider == null)
        {
            Debug.LogWarning("Editor plane collider is not assigned. Cannot determine plane height for deletion.");
            levelEditor.ShowMessage("Editor plane collider not found.", true);
            return;
        }

        float planeBottomY = editorPlaneCollider.bounds.min.y;

        List<GameObject> objectsToDelete = new List<GameObject>();

        foreach (Transform child in levelObjectsParent)
        {
            if (child.position.y < planeBottomY)
            {
                objectsToDelete.Add(child.gameObject);
            }
        }

        int deletedCount = 0;
        foreach (GameObject obj in objectsToDelete)
        {
            DeleteObject(obj);
            deletedCount++;
        }
    }
}