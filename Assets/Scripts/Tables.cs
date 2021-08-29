public static class Tables 
{
    public static readonly Suit[] suitTable = new Suit[16]
    {
        Suit.Diamond, Suit.Heart, Suit.Diamond, Suit.Club,
        Suit.Heart, Suit.Heart, Suit.Spade, Suit.Club,
        Suit.Heart, Suit.Diamond, Suit.Spade, Suit.Club,
        Suit.Club, Suit.Spade, Suit.Spade, Suit.Diamond
    };
    public static readonly Rank[] rankTable = new Rank[16]
    {
        Rank.Queen, Rank.Ten, Rank.Jack, Rank.Jack,
        Rank.King, Rank.Ace, Rank.Ten, Rank.King,
        Rank.Queen, Rank.Ace, Rank.Queen, Rank.Ten,
        Rank.Ten, Rank.Jack, Rank.King, Rank.Ace
    };  
}
