using UnityEngine;

public class VictoryCollectable : Collectable
{
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager �� �������� �� ����!");
        }
    }

    public override void OnBeingCollectedBy(MainPlayer character)
    {
        character.OnPlayerWon();
        uiManager?.ShowVictoryScreen();
        Destroy(gameObject);
    }
}
