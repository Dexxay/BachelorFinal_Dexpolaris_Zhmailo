using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private string chooseLevel = "GameLevelSelector";
    [SerializeField] private string chooseInfo = "InfoMenu";
    [SerializeField] private string chooseEditor = "LevelEditorTemplate";



    public void ChooseLevel()
    {
        SceneManager.LoadScene(chooseLevel);
    }

    public void ChooseInfo()
    {
        SceneManager.LoadScene(chooseInfo);
    }

    public void ChooseEditor()
    {
        SceneManager.LoadScene(chooseEditor);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
