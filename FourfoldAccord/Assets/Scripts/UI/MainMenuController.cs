using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject guidancePanel;

    private void Start()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowGuidance()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(true);
        }
    }

    public void HideGuidance()
    {
        if (guidancePanel != null)
        {
            guidancePanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}