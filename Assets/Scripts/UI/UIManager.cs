using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MainPlayer player;
    [SerializeField] private GameObject[] PlayerUI;
    [SerializeField] private GameObject[] objectsToShowAtDeath;
    [SerializeField] private GameObject[] objectsToShowAtPause;
    [SerializeField] private GameObject[] objectsToShowAtDamage;
    [SerializeField] private GameObject[] objectsToShowAtHeal;
    [SerializeField] private GameObject[] objectsToShowAtVictory;
    [SerializeField] private string playMenuSceneName = "GameLevelSelector";

    [SerializeField] private float damageDelay;

    void Start()
    {
        player.PlayerHealthManager.PlayerDied += OnPlayerDie;
        player.PlayerHealthManager.PlayerDamaged += OnPlayerDamaged;
        player.PlayerHealthManager.PlayerHealed += OnPlayerHealed;
        PauseManager.Instance.PauseStarted += OnPauseStarted;
        PauseManager.Instance.PauseEnded += OnPauseEnded;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(playMenuSceneName);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }

    private void OnPauseStarted()
    {
        foreach (var toDisable in PlayerUI)
        {
            toDisable.SetActive(false);
        }

        foreach (var toDisable in objectsToShowAtDamage)
        {
            toDisable.SetActive(false);
        }

        foreach (var toEnable in objectsToShowAtPause)
        {
            toEnable.SetActive(true);
        }
    }

    private void OnPauseEnded()
    {
        foreach (var toDisable in objectsToShowAtPause)
        {
            toDisable.SetActive(false);
        }

        foreach (var toEnable in PlayerUI)
        {
            toEnable.SetActive(true);
        }
    }

    private void OnPlayerDamaged()
    {
        foreach (var toEnable in objectsToShowAtDamage)
        {
            toEnable.SetActive(true);
        }
        Invoke("AfterPlayerDamaged", damageDelay);
    }

    void AfterPlayerDamaged()
    {
        foreach (var toDisable in objectsToShowAtDamage)
        {
            toDisable.SetActive(false);
        }
    }

    private void OnPlayerHealed()
    {
        foreach (var toEnable in objectsToShowAtHeal)
        {
            toEnable.SetActive(true);
        }
        Invoke("AfterPlayerHealed", damageDelay);
    }

    void AfterPlayerHealed()
    {
        foreach (var toDisable in objectsToShowAtHeal)
        {
            toDisable.SetActive(false);
        }
    }

    private void OnPlayerDie()
    {
        foreach (var toDisable in PlayerUI)
        {
            toDisable.SetActive(false);
        }

        foreach (var toDisable in objectsToShowAtDamage)
        {
            toDisable.SetActive(false);
        }

        foreach (var toDisable in objectsToShowAtPause)
        {
            toDisable.SetActive(false);
        }

        foreach (var toEnable in objectsToShowAtDeath)
        {
            toEnable.SetActive(true);
        }
    }

    public void ShowVictoryScreen()
    {
        foreach (var toDisable in PlayerUI)
        {
            toDisable.SetActive(false);
        }

        foreach (var toDisable in objectsToShowAtDamage)
        {
            toDisable.SetActive(false);
        }

        foreach (var toDisable in objectsToShowAtPause)
        {
            toDisable.SetActive(false);
        }

        foreach (var toEnable in objectsToShowAtVictory)
        {
            toEnable.SetActive(true);
        }
    }

}
