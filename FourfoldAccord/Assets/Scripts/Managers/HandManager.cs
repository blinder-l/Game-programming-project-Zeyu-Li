using System.Collections.Generic;
using System.Text;

public class HandManager
{
    private readonly List<PlayingCard> currentHand = new List<PlayingCard>();

    public IReadOnlyList<PlayingCard> CurrentHand => currentHand;
    public int HandSizeLimit { get; } = 8;
    public int CurrentHandCount => currentHand.Count;

    public void FillHand(DeckManager deckManager)
    {
        int cardsNeeded = HandSizeLimit - currentHand.Count;

        if (cardsNeeded <= 0)
        {
            return;
        }

        currentHand.AddRange(deckManager.DrawCards(cardsNeeded));
    }

    // Builds a readable list of cards currently in hand for console debugging.
    public string GetHandDebugText()
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < currentHand.Count; i++)
        {
            builder.AppendLine($"{i + 1}. {currentHand[i].GetDisplayName()}");
        }

        return builder.ToString();
    }
}
