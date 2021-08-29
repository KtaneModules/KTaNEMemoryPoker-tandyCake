using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using Rnd = UnityEngine.Random;

public class PuzzleGenerator
{
    public string allPuzzlesText { private get; set; }
    private static CardInfo[][] allPuzzles;

    public CardInfo[] chosenPuzzle;
    public List<int> givens;
    
    public void GetGrid()
    {
        if (allPuzzles == null)
        {
            allPuzzles = allPuzzlesText.Split('|').Select(puz =>
            puz.Split(',').Select(cell =>
            new CardInfo(cell)).ToArray()
            ).ToArray();
        }
        chosenPuzzle = allPuzzles.PickRandom();
    }
    public void GenerateGivens()
    {
        //Code haphazardly taken from Kudosudoku.
        int[] potentialGivens = Enumerable.Range(0, 16).ToArray().Shuffle();
        givens = Ut.ReduceRequiredSet(potentialGivens, state =>
        allPuzzles.Count(puzzle =>
        state.SetToTest.All(ix => puzzle[ix].Equals(chosenPuzzle[ix]))) == 1
        ).ToList();

        Debug.Log(givens.Join());
        Debug.Log(allPuzzles.Count(puzzle => Enumerable.Range(0, 16).All(ix => puzzle[ix].Equals(chosenPuzzle[ix]))));
    }

    public void GetAllPuzzles()
    {
        int[][] grids = new int[][]{
        new int[]{ 0, 1, 2, 3, 2, 3, 0, 1, 3, 2, 1, 0, 1, 0, 3, 2 },
        new int[]{ 0, 1, 2, 3, 3, 2, 1, 0, 1, 0, 3, 2, 2, 3, 0, 1 } };
        int[][] mappings = new int[][] {
            new[] { 0, 1, 2, 3 }, new[] {0, 1, 3, 2 }, new[] { 0, 2, 1, 3 }, new[] { 0, 2, 3, 1 }, new[]{0, 3, 1, 2 }, new[] {0, 3, 2, 1 },
            new[] { 1, 2, 3, 0 }, new[] {1, 2, 0, 3 }, new[] { 1, 3, 2, 0 }, new[] { 1, 3, 0, 2 }, new[]{1, 0, 2, 3 }, new[] {1, 0, 3, 2 },
            new[] { 2, 3, 0, 1 }, new[] {2, 3, 1, 0 }, new[] { 2, 0, 3, 1 }, new[] { 2, 0, 1, 3 }, new[]{2, 1, 3, 0 }, new[] {2, 1, 0, 3 },
            new[] { 3, 0, 1, 2 }, new[] {3, 0, 2, 1 }, new[] { 3, 1, 0, 2 }, new[] { 3, 1, 2, 0 }, new[]{3, 2, 0, 1 }, new[] {3, 2, 1, 0 }
            };
        HashSet<string> puzzles = new HashSet<string>();
        for (int grid = 0; grid < 2; grid++)
            for (int rot = 0; rot < 4; rot++)
                for (int refl = 0; refl < 4; refl++)
                    for (int rankMapping = 0; rankMapping < 24; rankMapping++)
                        for (int suitMapping = 0; suitMapping < 24; suitMapping++)
                        {
                            int[] rankGrid = TransformationHandler.ApplyTransformation(grids[grid],
                                    (Rotation)rot, (Reflection)refl).Select(x => mappings[rankMapping][x]).ToArray();
                            int[] suitGrid = TransformationHandler.ApplyTransformation(grids[1 - grid],
                                    (Rotation)rot, (Reflection)refl).Select(x => mappings[suitMapping][x]).ToArray();
                            puzzles.Add(Enumerable.Range(0, 16).Select(pos =>
                                 "" + "JQKA"[rankGrid[pos]] + "♠♥♣♦"[suitGrid[pos]])
                                    .Join(","));
                        }
        string path = "C:/Users/danie/OneDrive/Documents/GitHub/KTaNEMemoryPoker/Assets/AllPuzzles.txt";
        File.WriteAllText(path, puzzles.Join("|"));
        Debug.LogFormat("Successfully generated {0} puzzles.", puzzles.Count);
    }
}
