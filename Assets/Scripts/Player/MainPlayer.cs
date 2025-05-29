using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerHealthManager playerHealthManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseMovement mouseMovement;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private MapTimer mapTimer;

    public WeaponManager WeaponManager => weaponManager;
    public PlayerHealthManager PlayerHealthManager => playerHealthManager;
    public PlayerMovement PlayerMovement => playerMovement;
    public MouseMovement MouseMovement => mouseMovement;
    public ScoreManager ScoreManager => scoreManager;
    public CharacterController CharacterController => characterController;
    public MapTimer MapTimer => mapTimer;

    private void Start()
    {
        PlayerHealthManager.PlayerDied += OnPlayerDied;
        PauseManager.Instance.PauseStarted += OnPauseStarted;
        PauseManager.Instance.PauseEnded += OnPauseEnded;
    }


    private void OnPauseStarted()
    {
        scoreManager.enabled = false;
        playerMovement.enabled = false;
        mouseMovement.enabled = false;
        characterController.enabled = false;
        weaponManager.SelectedWeapon.gameObject.SetActive(false);
        weaponManager.enabled = false;
    }

    private void OnPauseEnded()
    {
        scoreManager.enabled = true;
        playerMovement.enabled = true;
        mouseMovement.enabled = true;
        characterController.enabled = true;
        weaponManager.SelectedWeapon.gameObject.SetActive(true);
        weaponManager.enabled = true;
    }


    private void OnPlayerDied()
    {
        scoreManager.enabled = false;
        playerMovement.enabled = false;
        mouseMovement.enabled = false;
        characterController.enabled = false;
        weaponManager.SelectedWeapon.gameObject.SetActive(false);
        weaponManager.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0;
    }

    public void OnPlayerWon()
    {
        scoreManager.enabled = false;
        playerMovement.enabled = false;
        mouseMovement.enabled = false;
        characterController.enabled = false;
        weaponManager.SelectedWeapon.gameObject.SetActive(false);
        weaponManager.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0;
    }
}
