using System.Collections;
using System.Collections.Generic;
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
