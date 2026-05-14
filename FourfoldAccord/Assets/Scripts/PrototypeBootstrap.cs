using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private void Start()
    {
        DeckManager deckManager = new DeckManager();
        deckManager.CreateStandardDeck();

        Debug.Log("Prototype started");
        Debug.Log($"Deck count: {deckManager.DrawPileCount}");

        int previewCount = Mathf.Min(5, deckManager.DrawPileCount);

        for (int i = 0; i < previewCount; i++)
        {
            Debug.Log($"Card {i + 1}: {deckManager.DrawPile[i].GetDisplayName()}");
        }
    }
}
