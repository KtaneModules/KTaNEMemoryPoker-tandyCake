using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class CardInfo : MonoBehaviour, ICloneable {

	public Suit suit { get; set; }
	public Rank rank { get; set; }

    public object Clone()
    {
        return new CardInfo(suit, rank);
    }
    public CardInfo(Suit st, Rank rk)
    {
        suit = st;
        rank = rk;
    }
    public CardInfo(string str)
    {
        if (!Regex.IsMatch(str, @"^[TJQKA][♠♥♣♦]$"))
            throw new FormatException();
        suit = (Suit) "JQKAT".IndexOf(str[0]);
        rank = (Rank)"♠♥♣♦".IndexOf(str[1]);
    }
    public CardInfo() { }

    public override string ToString()
    {
        return "" + rank.ToString()[0] + "♠♥♣♦"[(int)suit];
    }
    public override bool Equals(object other)
    {
        return other is CardInfo && (other as CardInfo).suit == suit && (other as CardInfo).rank == rank;
    }

    public static IEnumerable<CardInfo> FormGrid(Suit[] suits, Rank[] ranks)
    {
        for (int i = 0; i < suits.Length; i++)
            yield return new CardInfo() { suit = suits[i], rank = ranks[i] };
    }
}