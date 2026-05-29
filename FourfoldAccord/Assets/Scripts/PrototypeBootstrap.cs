using UnityEngine;

public class PrototypeBootstrap : MonoBehaviour
{
    private void Start()
    {
        DeckManager deckManager = new DeckManager();
        deckManager.CreateStandardDeck();

        Debug.Log("Prototype started");
        Debug.Log($"Deck count before shuffle: {deckManager.DrawPileCount}");
        Debug.Log($"Top card before shuffle: {deckManager.DrawPile[0].GetDisplayName()}");

        deckManager.Shuffle();
        Debug.Log("Deck shuffled");

        HandManager handManager = new HandManager();
        handManager.FillHand(deckManager);

        Debug.Log($"Hand size limit: {handManager.HandSizeLimit}");
        Debug.Log($"Current hand count: {handManager.CurrentHandCount}");
        Debug.Log($"Deck count after filling hand: {deckManager.DrawPileCount}");
        Debug.Log($"Current hand:\n{handManager.GetHandDebugText()}");
    }
}
