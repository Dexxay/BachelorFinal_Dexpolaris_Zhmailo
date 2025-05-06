using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class LevelMenu : MonoBehaviour
{
    [SerializeField] private Toggle [] toggles;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartLevel();
        }
    }

    public void StartLevel()
    {
        if (toggles[0].isOn)
        {
            SceneManager.LoadScene(3);
        }
        else if (toggles[1].isOn)
        {
            SceneManager.LoadScene(4);
        }
        else if (toggles[2].isOn)
        {
            SceneManager.LoadScene(5);
        }
        else
        {
            SceneManager.LoadScene(3);
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
