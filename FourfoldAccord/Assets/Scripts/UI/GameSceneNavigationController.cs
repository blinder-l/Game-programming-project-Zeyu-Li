using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneNavigationController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}