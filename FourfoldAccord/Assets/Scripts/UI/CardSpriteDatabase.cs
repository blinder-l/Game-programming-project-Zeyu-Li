using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardSpriteDatabase : MonoBehaviour
{
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private string editorCardSpriteFolder = "Assets/Spirites/Cards";
    [SerializeField] private CardSpriteEntry[] cardSprites = new CardSpriteEntry[0];

    public Sprite CardBackSprite => cardBackSprite;

    private void Awake()
    {
        EnsureCardSpritesPopulated();
    }

    public Sprite GetSprite(PlayingCard card)
    {
        if (card == null)
        {
            return cardBackSprite;
        }

        return GetSprite(card.suit, card.rank);
    }

    public Sprite GetSprite(Suit suit, Rank rank)
    {
        EnsureCardSpritesPopulated();

        if (cardSprites == null)
        {
            return cardBackSprite;
        }

        for (int i = 0; i < cardSprites.Length; i++)
        {
            CardSpriteEntry entry = cardSprites[i];

            if (entry.suit == suit && entry.rank == rank)
            {
                return entry.sprite;
            }
        }

        return cardBackSprite;
    }

    private void EnsureCardSpritesPopulated()
    {
#if UNITY_EDITOR
        if (cardSprites == null || cardSprites.Length == 0)
        {
            AutoPopulateCardSprites();
        }
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cardSprites == null || cardSprites.Length == 0)
        {
            AutoPopulateCardSprites();
        }
    }

    [ContextMenu("Auto Populate Card Sprites")]
    private void AutoPopulateCardSprites()
    {
        List<CardSpriteEntry> entries = new List<CardSpriteEntry>();

        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                string fileName = GetSpriteFileName(suit, rank);
                string path = $"{editorCardSpriteFolder}/{fileName}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                entries.Add(new CardSpriteEntry(suit, rank, sprite));
            }
        }

        cardSprites = entries.ToArray();
    }
#endif

    private string GetSpriteFileName(Suit suit, Rank rank)
    {
        return $"{GetSpriteSuitName(suit)} {GetSpriteRankName(rank)}";
    }

    private string GetSpriteSuitName(Suit suit)
    {
        switch (suit)
        {
            case Suit.Hearts:
                return "Heart";
            case Suit.Spades:
                return "Spade";
            case Suit.Diamonds:
                return "Diamond";
            default:
                return "Club";
        }
    }

    private string GetSpriteRankName(Rank rank)
    {
        switch (rank)
        {
            case Rank.Ace:
                return "A";
            case Rank.King:
                return "K";
            case Rank.Queen:
                return "Q";
            case Rank.Jack:
                return "J";
            case Rank.Ten:
                return "10";
            case Rank.Nine:
                return "9";
            case Rank.Eight:
                return "8";
            case Rank.Seven:
                return "7";
            case Rank.Six:
                return "6";
            case Rank.Five:
                return "5";
            case Rank.Four:
                return "4";
            case Rank.Three:
                return "3";
            default:
                return "2";
        }
    }
}

[Serializable]
public class CardSpriteEntry
{
    public Suit suit;
    public Rank rank;
    public Sprite sprite;

    public CardSpriteEntry(Suit suit, Rank rank, Sprite sprite)
    {
        this.suit = suit;
        this.rank = rank;
        this.sprite = sprite;
    }
}
