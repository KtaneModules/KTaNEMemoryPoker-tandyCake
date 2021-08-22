using System;
public static class TransformationHandler
{
    public static T[] ApplyTransformation<T>(T[] grid, Rotation rotation = Rotation.none, Reflection reflection = Reflection.none)
    {
        if (grid.Length != 16)
            throw new ArgumentOutOfRangeException("grid.Length", grid.Length, "Grid size is not equal to 16.");
        T[] newGrid = new T[16];
        for (int i = 0; i < 16; i++)
            newGrid[ApplyRot(i, rotation)] = grid[i];
        grid = (T[])newGrid.Clone();
        for (int i = 0; i < 16; i++)
            newGrid[ApplyRef(i, reflection)] = grid[i];
        return newGrid;
    }
    public static int[] ApplyAdd(int[] grid, int adder)
    {
        for (int i = 0; i < 16; i++)
            grid[i] = (grid[i] + adder) % 4;
        return grid;
    }
    private static int ApplyRot(int pos, Rotation appliedRot)
    {
        int x = pos % 4;
        int y = pos / 4;
        switch (appliedRot)
        {
            case Rotation.none:
                return Flatten(x, y);
            case Rotation.ninetyCW:
                return Flatten(3 - y, x);
            case Rotation.hundredEightyCW:
                return Flatten(3 - x, 3 - y);
            case Rotation.ninetyCCW:
                return Flatten(y, 3 - x);
            default:
                throw new ArgumentOutOfRangeException("appliedRot", appliedRot.ToString(), "Applied rotation is not one known.");
        }
    }
    private static int ApplyRef(int pos, Reflection appliedRef)
    {
        int x = pos % 4;
        int y = pos / 4;
        switch (appliedRef)
        {
            case Reflection.none:
                return Flatten(x, y);
            case Reflection.X:
                return Flatten(x, 3 - y);
            case Reflection.Y:
                return Flatten(3 - x, y);
            case Reflection.XY:
                return Flatten(3 - x, 3 - y);
            case Reflection.YX:
                return Flatten(y, x);
            default:
                throw new ArgumentOutOfRangeException("appliedRef", appliedRef.ToString(), "Applied reflection is not one known.");
        }
    }
    private static int Flatten(int x, int y)
    {
        return 4 * y + x;
    }
}
