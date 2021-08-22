using System;
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
}