using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic base grid to hold just the important part of the grid creation
/// </summary>
public class BaseGrid<T>
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int CellSize { get; private set; }
    protected T[,] gridArray;
    private Dictionary<T, (int x, int y)> valuePositions;

    public BaseGrid(int _width, int _height, int _cellSize)
    {
        Width = _width;
        Height = _height;
        CellSize = _cellSize;
        gridArray = new T[Width, Height];
        valuePositions = new();
    }

    public virtual Vector2 SetValue(int x, int y, T value)
    {
        if (!IsPositionInside(x, y) && TryGetPosition(value, out int curX, out int curY))
        {
            return new Vector2(curX, curY);
        }

        if (value != null)
        {
            if (valuePositions.TryGetValue(value, out var oldPos))
            {
                // Clear old position
                gridArray[oldPos.x, oldPos.y] = default(T);
            }

            // Set new position
            gridArray[x, y] = value;
            valuePositions[value] = (x, y);
        }
        else
        {
            // Removing value
            //Debug.Log("Removing value from grid " + x + "/"+ y);
            T oldValue = gridArray[x, y];
            if (!IsNull(oldValue))
            {
                valuePositions.Remove(oldValue);
            }

            gridArray[x, y] = default(T);
        }

        return new Vector2(x, y);
    }


    public virtual bool TryGetValue(int x, int y, out T gridResult)
    {
        gridResult = default(T);
        if (IsPositionInside(x, y))
        {
            gridResult = gridArray[x, y];
        }

        return !IsNull(gridResult);
    }

    public bool TryGetPosition(T value, out int x, out int y)
    {
        if (valuePositions.TryGetValue(value, out var pos))
        {
            x = pos.x;
            y = pos.y;
            return true;
        }
        x = -1;
        y = -1;
        return false;
    }

    public bool IsPositionInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public virtual bool IsPositionFilled(int x, int y)
    {
        if (!TryGetValue(x, y, out T result))
        {
            return false;
        }

        return !IsNull(result);
    }

    private bool IsNull(T result) => Equals(result, default(T));


    /// <summary>
    /// Convert list index to the grid position
    /// </summary>
    /// <param name="index"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public (int x, int y) IndexToGrid(int index, UnityEngine.UI.GridLayoutGroup layout)
    {
        int x = 0;
        int y = 0;

        switch (layout.constraint)
        {
            case UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount:
                x = index % layout.constraintCount;
                y = index / layout.constraintCount;
                break;
            case UnityEngine.UI.GridLayoutGroup.Constraint.FixedRowCount:
                x = index / layout.constraintCount;
                y = index % layout.constraintCount;
                break;
            default:
                // Handle Flexible constraint if necessary
                break;
        }

        // Adjust based on startCorner
        switch (layout.startCorner)
        {
            case UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft:
                // No adjustment needed
                break;
            case UnityEngine.UI.GridLayoutGroup.Corner.UpperRight:
                x = layout.constraintCount - 1 - x;
                break;
            case UnityEngine.UI.GridLayoutGroup.Corner.LowerLeft:
                y = layout.constraintCount - 1 - y;
                break;
            case UnityEngine.UI.GridLayoutGroup.Corner.LowerRight:
                x = layout.constraintCount - 1 - x;
                y = layout.constraintCount - 1 - y;
                break;
        }

        return (x, y);
    }


    /// <summary>
    /// Convert the grid to a list index
    /// </summary>
    public int GridToIndex(int x, int y, UnityEngine.UI.GridLayoutGroup layout)
    {
        int index = 0;
        int constraintCount = layout.constraintCount;

        switch (layout.constraint)
        {
            case UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount:
                switch (layout.startCorner)
                {
                    case UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft:
                        index = y * constraintCount + x;
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.UpperRight:
                        index = y * constraintCount + (constraintCount - 1 - x);
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.LowerLeft:
                        index = (layout.transform.childCount / constraintCount - 1 - y) * constraintCount + x;
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.LowerRight:
                        index = (layout.transform.childCount / constraintCount - 1 - y) * constraintCount + (constraintCount - 1 - x);
                        break;
                }
                break;

            case UnityEngine.UI.GridLayoutGroup.Constraint.FixedRowCount:
                switch (layout.startCorner)
                {
                    case UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft:
                        index = x * constraintCount + y;
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.UpperRight:
                        index = (layout.transform.childCount / constraintCount - 1 - x) * constraintCount + y;
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.LowerLeft:
                        index = x * constraintCount + (constraintCount - 1 - y);
                        break;
                    case UnityEngine.UI.GridLayoutGroup.Corner.LowerRight:
                        index = (layout.transform.childCount / constraintCount - 1 - x) * constraintCount + (constraintCount - 1 - y);
                        break;
                }
                break;

            default:
                // Handle Flexible constraint if necessary
                break;
        }

        return index;
    }

}
