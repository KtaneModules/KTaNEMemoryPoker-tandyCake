using System;
using System.Collections.Generic;
using UnityEngine;

public class CardInfo : MonoBehaviour, ICloneable {

	public Suit suit { get; set; }
	public Rank rank { get; set; }

    public object Clone()
    {
        return new CardInfo() { suit = suit, rank = rank };
    }

    public override string ToString()
    {
        return "" + rank.ToString()[0] + "♠♥♣♦"[(int)suit];
    }
    public static IEnumerable<CardInfo> FormGrid(Suit[] suits, Rank[] ranks)
    {
        for (int i = 0; i < suits.Length; i++)
            yield return new CardInfo() { suit = suits[i], rank = ranks[i] };
    }
}