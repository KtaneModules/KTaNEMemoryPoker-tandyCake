using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class PuzzleGenerator
{
    public List<int> givens = new List<int>();
    private static readonly int[,][] grids = new int[,][]
    {
       { new int[] { 0, 1, 2, 3, 3, 2, 1, 0, 1, 0, 3, 2, 2, 3, 0, 1 }, new int[] {0, 1, 2, 3, 2, 3, 0, 1, 3, 2, 1, 0, 1, 0, 3, 2 } },
       { new int[] { 0, 1, 2, 3, 2, 3, 0, 1, 3, 2, 1, 0, 1, 0, 3, 2 }, new int[] {0, 1, 2, 3, 3, 2, 1, 0, 1, 0, 3, 2, 2, 3, 0, 1 } },
    };
    private static readonly int[][] uniqueLines = new[]
    {
        new[] { 0, 1, 2, 3 }, new[] {4,5,6,7 }, new[] {8,9,10,11 }, new[] {12,13,14,15 },
        new[] {0,4,8,12 }, new[] {1,5,9,13 }, new[] {2,6,10,14 }, new[] {3,7,11,15 },
        new[] {0,5,10,15 }, new[] {3,6,9,12 }
    };

    private int[] suitGridInit;
    private int[] rankGridInit;
    private Rotation rot;
    private Reflection refl;

    public Suit[] suitGrid;
    public Rank[] rankGrid;
    
    public void CreateGrid()
    {
        int chosenGrid = Rnd.Range(0, 2);
        int swap = Rnd.Range(0, 2);
        suitGridInit = grids[chosenGrid, swap];
        rankGridInit = grids[chosenGrid, 1 - swap];
        GetTransformations();
        suitGrid = TransformationHandler.ApplyTransformation(suitGridInit, rot, refl).Cast<Suit>().ToArray();
        suitGrid = TransformationHandler.ApplyAdd(suitGrid.Cast<int>().ToArray(), Rnd.Range(0, 4)).Cast<Suit>().ToArray();
        rankGrid = TransformationHandler.ApplyTransformation(rankGridInit, rot, refl).Cast<Rank>().ToArray();
        rankGrid = TransformationHandler.ApplyAdd(rankGrid.Cast<int>().ToArray(), Rnd.Range(0, 4)).Cast<Rank>().ToArray();
    }
    public void GenerateGivens()
    {
        int[] potentialGivens = Enumerable.Range(0, 16).ToArray().Shuffle();
        givens = Ut.ReduceRequiredSet(potentialGivens, state =>
                state.SetToTest.All()
    }

    private void GetTransformations()
    {
        rot = (Rotation)Rnd.Range(0, 4);
        refl = (Reflection)Rnd.Range(0, 5);
    }
    private bool ValidSudoku(Suit[] suits, Rank[] ranks)
    {
        if (CardInfo.FormGrid(suits, ranks).Distinct().Count() != 16)
            return false;
        foreach (int[] line in uniqueLines)
            if (line.Select(x => suits[x]).Distinct().Count() != 4 || line.Select(x => ranks[x]).Distinct().Count() != 4)
                return false;
        return true;
    }
    private bool ValidSudoku(CardInfo[] cards)
    {
        return ValidSudoku(cards.Select(x => x.suit).ToArray(), cards.Select(x => x.rank).ToArray());
    }
}
