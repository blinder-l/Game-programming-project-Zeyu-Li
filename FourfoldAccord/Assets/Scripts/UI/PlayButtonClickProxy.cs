using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PlayButtonClickProxy : MonoBehaviour
{
    private Button button;
    private GameUIController gameUIController;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("PlayButtonClickProxy failed: Button component missing.");
            return;
        }

        button.interactable = true;

        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }

        if (FindObjectOfType<GameUIController>() != null)
        {
            Debug.Log("PlayButtonClickProxy skipped binding because GameUIController owns PlayButton.");
            return;
        }

        button.onClick.RemoveListener(OnPlayButtonClicked);
        button.onClick.AddListener(OnPlayButtonClicked);
        Debug.Log("PlayButtonClickProxy bound directly on PlayButton as fallback.");
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("UI PlayButton clicked via PlayButtonClickProxy.");

        if (gameUIController == null)
        {
            gameUIController = FindObjectOfType<GameUIController>();
        }

        if (gameUIController == null)
        {
            Debug.LogError("PlayButtonClickProxy failed: GameUIController not found.");
            return;
        }

        gameUIController.HandlePlayButtonClicked();
    }
}
