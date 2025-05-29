using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
