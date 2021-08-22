using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PokerHandCalculator  {

    public List<CardInfo> hand = new List<CardInfo>();
    public CardInfo[] grid { private get; set; }
    public PokerHand bestHand { get; private set; }
    public CardInfo[] bestHandSet { get; private set; }
    public PokerHandCalculator(CardInfo[] cards)
    {
        grid = cards;
    }
    public PokerHandCalculator() { }

    public void Add(CardInfo card)
    {
        hand.Add(card);
    }

    public PokerHand CalculateHand(IEnumerable<CardInfo> hand)
    {
        Suit[] suits = hand.Select(x => x.suit).ToArray();
        Rank[] ranks = hand.Select(x => x.rank).ToArray();
        Suit[] differentSuits = suits.Distinct().ToArray();
        Rank[] differentRanks = ranks.Distinct().ToArray();
        int[] rankCounts = GetRankCounts(hand);

        if (differentRanks.Length == 5 && differentSuits.Length == 1) //One of each rank and all of the same suit
            return PokerHand.Royal_Flush;
        //Straight flush is excluded because it is indistinguishable from a royal flush with TJQKA.
        else if (rankCounts.Any(x => x == 4)) //Any rank appears 4 times
            return PokerHand.Four_Of_A_Kind;
        else if (rankCounts.Any(x => x == 3) && rankCounts.Any(x => x == 2)) //A rank appears 3 times and a different rank appears 2 times
            return PokerHand.Full_House;
        else if (differentSuits.Length == 1) //All of the same suit
            return PokerHand.Flush;
        else if (differentRanks.Length == 5) //Any sequence of TJQKA will always be a straight.
            return PokerHand.Straight;
        else if (rankCounts.Any(x => x == 3)) //Any rank appears 3 times
            return PokerHand.Three_Of_A_Kind;
        else if (rankCounts.Count(x => x == 2) == 2) //Two ranks appear twice.
            return PokerHand.Two_Pair;
        else return PokerHand.Pair;
        //A high card case will never occur because having 5 unique cards will result in a straight.
    }
    public void GetBestHand(IEnumerable<int> excludedPositions)
    {
        Dictionary<CardInfo[], PokerHand> possibleHands = new Dictionary<CardInfo[], PokerHand>();
        CardInfo[] AvailableCards = grid.Where((_, pos) => !excludedPositions.Contains(pos)).ToArray();
        for (int i = 0; i < AvailableCards.Length; i++)
            for (int j = 0; j < AvailableCards.Length; j++)
                if (i != j)
                {
                    CardInfo[] hnd = hand.Concat(new CardInfo[] { AvailableCards[i], AvailableCards[j] }).ToArray();
                    possibleHands.Add(hnd, CalculateHand(hnd));
                };
        bestHand = possibleHands.Max(x => x.Value);
        bestHandSet = possibleHands.Where(x => x.Value == bestHand).PickRandom().Key;
    }
    private int[] GetRankCounts(IEnumerable<CardInfo> cards)
    {
        return Enumerable.Range(0, 5).Cast<Rank>().Select(rnk => cards.Select(x => x.rank).Count(x => x == rnk)).ToArray();
    }
}
