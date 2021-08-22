using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class PuzzleGenerator
{
    private int[] grid = new int[16];
    private static readonly int[,][] grids = new int[,][]
    {
       { new int[] { 0, 1, 2, 3, 3, 2, 1, 0, 1, 0, 3, 2, 2, 3, 0, 1 }, new int[] {0, 1, 2, 3, 2, 3, 0, 1, 3, 2, 1, 0, 1, 0, 3, 2 } },
       { new int[] { 0, 1, 2, 3, 2, 3, 0, 1, 3, 2, 1, 0, 1, 0, 3, 2 }, new int[] {0, 1, 2, 3, 3, 2, 1, 0, 1, 0, 3, 2, 2, 3, 0, 1 } },
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
    private void GetTransformations()
    {
        rot = (Rotation)Rnd.Range(0, 4);
        refl = (Reflection)Rnd.Range(0, 5);
    }
}
