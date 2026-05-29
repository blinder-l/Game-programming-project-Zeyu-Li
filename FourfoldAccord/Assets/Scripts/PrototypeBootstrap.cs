using System.Collections.Generic;
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
        Debug.Log($"Top card after shuffle: {deckManager.DrawPile[0].GetDisplayName()}");

        List<PlayingCard> drawnCards = deckManager.DrawCards(5);
        Debug.Log($"Drew {drawnCards.Count} cards");

        for (int i = 0; i < drawnCards.Count; i++)
        {
            Debug.Log($"Drawn card {i + 1}: {drawnCards[i].GetDisplayName()}");
        }

        Debug.Log($"Deck count after draw: {deckManager.DrawPileCount}");
    }
}
