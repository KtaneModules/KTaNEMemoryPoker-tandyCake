using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class MemoryPokerScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public Card[] cards;
    public Sprite[] sprites;
    public TextAsset allPuzzles;
   

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private bool uninteractable;
    private static readonly string[] coordinates = { "A1", "B1", "C1", "D1", "A2", "B2", "C2", "D2", "A3", "B3", "C3", "D3", "A4", "B4", "C4", "D4" };

    private PuzzleGenerator generator = new PuzzleGenerator();
    private PokerHandCalculator handCalc = new PokerHandCalculator();
    private CardInfo[] startingGrid = new CardInfo[16];
    private int[] initiallyFaceUp;
    private int[] tableCards;
    private int[] originalPositions;
    private int stage = 0;
    private const int STAGE_COUNT = 3;
    private bool startingPhase = true;

    private CardInfo[] actualCards = new CardInfo[3];

    private Suit[] suitTable;
    private Rank[] rankTable;
    private bool currentToSuit;

    void Awake () {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 16; i++)
        {
            int ix = i;
            cards[ix].selectable.OnInteract += delegate () { CardPress(ix); return false; };
        }

        Module.OnActivate += delegate () { StartCoroutine(Activate()); } ;
        generator.allPuzzlesText = allPuzzles.text;
    }
    void Start ()
    {   
        generator.GetGrid();
        generator.GenerateGivens();

        startingGrid = generator.chosenPuzzle;
        initiallyFaceUp = generator.givens.OrderBy(x => x).ToArray();

        
        for (int i = 0; i < 16; i++)
            cards[i].Info = startingGrid[i];


        handCalc.grid = (CardInfo[])startingGrid.Clone();
        DetermineGrids();

        Log("The face-up cards are:");
        LogGrid(Enumerable.Range(0, 16).Select(x => initiallyFaceUp.Contains(x) ? startingGrid[x].ToString() : "..").ToArray());

        Log("The starting grid is:");
        LogGrid(startingGrid);

    }

    void DetermineGrids()
    {
        if (Bomb.GetBatteryCount() == 3 && Bomb.GetBatteryHolderCount() == 2)
        {
            Log("Table case 1 used.");
            rankTable = Tables.rankTable;
            suitTable = Tables.suitTable;
            currentToSuit = false;
        }
        else if (Bomb.GetSerialNumber().Contains('S') || Bomb.GetSerialNumber().Contains('G'))
        {
            Log("Table case 2 used.");
            rankTable = Tables.rankTable;
            suitTable = Tables.suitTable;
            currentToSuit = true;
        }
        else if (Bomb.GetPortPlates().Any(x => x.Length == 0))
        {
            Log("Table case 3 used.");
            rankTable = TransformationHandler.ApplyTransformation(Tables.rankTable, Rotation.ninetyCW).ToArray();
            suitTable = TransformationHandler.ApplyTransformation(Tables.suitTable, Rotation.ninetyCW).ToArray();
            currentToSuit = Bomb.GetPortCount() == 0;
        }
        else if (Bomb.GetIndicators().Count() >= 2)
        {
            Log("Table case 4 used.");
            string[] firstInds = Bomb.GetIndicators().OrderBy(x => x).Take(2).ToArray();
            if (Bomb.GetOnIndicators().Contains(firstInds[0]))
                rankTable = TransformationHandler.ApplyTransformation(Tables.rankTable, Rotation.none, Reflection.X);
            else rankTable = TransformationHandler.ApplyTransformation(Tables.rankTable, Rotation.hundredEightyCW);
            if (Bomb.GetOnIndicators().Contains(firstInds[1]))
                suitTable = TransformationHandler.ApplyTransformation(Tables.suitTable, Rotation.none, Reflection.X);
            else suitTable = TransformationHandler.ApplyTransformation(Tables.suitTable, Rotation.hundredEightyCW);
            currentToSuit = Bomb.GetIndicators().Any(ind => Bomb.GetIndicators().Where(x => x != ind).Any(other => ind.Any(letter => other.Contains(letter))));
        }
        else
        {
            Log("Table case 5 used.");
            int snSum = Bomb.GetSerialNumber().Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)).Sum();
            int ccwCount = (4 - (snSum % 3 + 1)) % 4; //Since rotations are implemented based on cw directions, to turn the ccw we need to do some shenanigans with subtracting from 4.
            rankTable = TransformationHandler.ApplyTransformation(Tables.rankTable, (Rotation)ccwCount);
            suitTable = TransformationHandler.ApplyTransformation(Tables.suitTable, (Rotation)ccwCount);
            currentToSuit = snSum / 10 % 2 == snSum % 2;
        }
        Log("The rank table used is:");
        LogGrid(rankTable.Select(x => x.ToString()[0].ToString()).ToArray());
        Log("The suit table used is:");
        LogGrid(suitTable.Select(x => "♠♥♣♦"[(int)x].ToString()).ToArray());
        Log(string.Format("The {0} table is used for the current positions, and the {1} table is used for the initial positions", 
            currentToSuit ? "suit" : "rank", currentToSuit ? "rank" : "suit"));
    }


    void CardPress(int pos)
    {
        if (cards[pos].animating || uninteractable)
            return;
        if (moduleSolved)
            cards[pos].Flip();
        else if (startingPhase)
            StartCoroutine(InitializeStage());
        else if (!cards[pos].faceUp)
        {
            Log(string.Format("Flipped {0} ({1})", coordinates[pos], cards[pos].Info));
            cards[pos].Flip();
            handCalc.Add(cards[pos].Info);
            if (cards.Count(x => x.faceUp) == 5)
                StartCoroutine(Submit());
        }
    }
    IEnumerator Activate()
    {
        uninteractable = true;
        for (int i = 0; i < 16; i++)
            cards[i].UpdateAppearance();
        yield return FlipMultiple(initiallyFaceUp.Select(x => cards[x]));
        uninteractable = false;
    }
    void LogGrid(object[] grid)
    {
        for (int i = 0; i < 4; i++)
            Log(Enumerable.Range(i * 4, 4).Select(x => grid[x]).Join());
    }

    IEnumerator InitializeStage()
    {
        uninteractable = true;
        yield return new WaitUntil(() => cards.All(x => !x.animating));
        stage++;
        yield return FlipMultiple(cards.Where(x => x.faceUp));
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 16; i++)
            cards[i].UpdateAppearance();
        yield return GenerateStage();
        startingPhase = false;
        uninteractable = false;
    }
    IEnumerator Submit()
    {
        uninteractable = true;
        yield return new WaitUntil(() => cards.All(c => !c.animating));
        PokerHand currentHand = handCalc.CalculateHand(handCalc.hand);
        Log("Submitted cards " + handCalc.hand.Join());
        Log("Submitted a " + currentHand.ToString().Replace('_', ' '));
        if (currentHand == handCalc.bestHand)
        {
            if (stage >= STAGE_COUNT)
            {
                yield return FlipMultiple(cards.Where(x => x.faceUp));
                moduleSolved = true;
                for (int i = 0; i < 16; i++)
                {
                    cards[i].Info = startingGrid[i];
                    cards[i].UpdateAppearance();
                }
                Module.HandlePass();
                Log("Module solved!");
                Audio.PlaySoundAtTransform("MPokSolveSound", transform);
                uninteractable = false;
            }
            else
            {
            for (int i = 0; i < 16; i++)
                cards[i].Info = startingGrid[i];
                handCalc.hand.Clear();
                Log("That was correct. Progressing to stage " + (stage + 1));
                yield return InitializeStage();
            }
        }
        else
        {
            handCalc.hand.Clear();
            Log("That was incorrect. Resetting stage.");
            StartCoroutine(Strike());
        }
    }

    IEnumerator GenerateStage()
    {
        yield return new WaitUntil(() => cards.All(x => !x.animating));
        GetTablePlacements();
        GetActualCards();
        handCalc.GetBestHand(tableCards);
        Log(string.Format("The best possible poker hand is a {0}, which is obtainable with cards {1}.", handCalc.bestHand.ToString().Replace('_', ' '), handCalc.bestHandSet.Join()));
        yield return FlipMultiple(tableCards.Select(x => cards[x]).Where(x => !x.faceUp));
    }
    void GetTablePlacements()
    {
        tableCards = Enumerable.Range(0, 16).ToArray().Shuffle().Take(3).ToArray();
        originalPositions = Enumerable.Range(0, 16).ToArray().Shuffle().Take(3).ToArray();
        Debug.Log("orig: " + originalPositions.Join());
        for (int i = 0; i < 3; i++)
        {
            int pos = tableCards[i];
            cards[pos].Info = startingGrid[originalPositions[i]];
            cards[pos].UpdateAppearance();
            Log(string.Format("Card {0} flipped over to reveal a {1}.", coordinates[pos], cards[pos].ToString()));
        }
    }
    void GetActualCards()
    {
        handCalc.hand.Clear();
        for (int i = 0; i < 3; i++)
        {
            actualCards[i] = new CardInfo(
                suitTable[currentToSuit ? tableCards[i] : originalPositions[i]],
                rankTable[!currentToSuit ? tableCards[i] : originalPositions[i]]
                );
            handCalc.Add(actualCards[i]);
            Log(string.Format("The card at {0} maps to {1}.", coordinates[tableCards[i]], actualCards[i]));
        }
    }

    IEnumerator FlipMultiple(IEnumerable<Card> cards)
    {
        foreach (Card card in cards)
        {
            card.Flip();
            yield return new WaitForSeconds(0.125f);
        }
    }
    IEnumerator Strike()
    {
        yield return new WaitUntil(() => cards.All(x => !x.animating));
        Module.HandleStrike();
        stage--;
        uninteractable = true;
        yield return FlipMultiple(cards.Where(x => x.faceUp));
        yield return new WaitUntil(() => cards.All(x => !x.animating));
        for (int i = 0; i < 16; i++)
        {
            cards[i].Info = startingGrid[i];
            cards[i].UpdateAppearance();
        }
        yield return new WaitForSeconds(0.5f);
        yield return FlipMultiple(initiallyFaceUp.Select(x => cards[x]));
        startingPhase = true;
        uninteractable = false;
    }
    
    void Log(string msg)
    {
        Debug.LogFormat("[Memory Poker #{0}] {1}", moduleId, msg);
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} start> to begin the module. Use <!{0} flip A4 B3 C2 D1> to flip those cards. Letters represent columns and numbers represents rows, starting from A1 in the top-left.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:FLIP\s+)?((?:[A-D][1-4]\s*)+)$");
        if (command == "START" && startingPhase)
        {
            yield return null;
            cards.PickRandom().selectable.OnInteract();
        }
        else if (m.Success)
        {
            yield return null;
            foreach (string item in m.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                if (coordinates.Contains(item))
                {
                    cards[Array.IndexOf(coordinates, item)].selectable.OnInteract();
                    yield return new WaitForSeconds(0.125f);
                }
        }
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        while (uninteractable)
            yield return true;
        if (startingPhase)
            cards.PickRandom().selectable.OnInteract();
        yield return new WaitForSeconds(0.125f);
        while (true)
        {
            while (uninteractable)
                yield return true;
            if (moduleSolved)
                break;
            foreach (CardInfo card in handCalc.bestHandSet.Skip(3))
            {
                int pos = Enumerable.Range(0, 16).First(x => startingGrid[x].suit == card.suit && startingGrid[x].rank == card.rank);
                cards[pos].selectable.OnInteract();
                yield return new WaitForSeconds(0.125f);
            }
            yield return null;
        }
    }
}
