using UnityEngine;

public class EditorInputHandler : MonoBehaviour
{
    private LevelEditor levelEditor;
    private EditorObjectPlacement placementHandler;
    private EditorObjectDeleter deleter;
    private EditorPlacementPreviewHandler previewHandler;

    public void Init(LevelEditor editor, EditorObjectPlacement placement, EditorObjectDeleter objDeleter, EditorPlacementPreviewHandler preview)
    {
        levelEditor = editor;
        placementHandler = placement;
        deleter = objDeleter;
        previewHandler = preview;
    }

    public void Update()
    {
        if (levelEditor == null || levelEditor.UiHandler == null) return;

        if (levelEditor.UiHandler.IsPauseMenuOpen() || levelEditor.UiHandler.IsConfirmationDialogOpen())
        {
            previewHandler.DestroyPreview();
            return;
        }

        HandleKeyboardInput();
        HandleMouseInput();

        previewHandler.UpdatePreview(placementHandler.CurrentObjectToPlace, placementHandler.CurrentPreviewRotation, placementHandler.SelectedRandomAsteroidPrefab, Input.mousePosition);
    }

    private void HandleKeyboardInput()
    {
        placementHandler.HandleObjectSelection();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            levelEditor.ClearMessage();
        }

        if (Input.GetMouseButtonDown(0))
        {
            placementHandler.AttemptPlacement(Input.mousePosition);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            deleter.AttemptDeletion(Input.mousePosition);
        }
    }
}