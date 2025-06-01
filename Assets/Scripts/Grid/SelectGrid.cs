using System;
using UnityEngine;

public class SelectGrid<T> : BaseGrid<T>
{
    private Vector2Int curSelected;

    public event Action<(Vector2Int oldSelect, Vector2Int newSelect)> OnSelectChange;

    public SelectGrid(int _width, int _height) : base(_width, _height, 1)
    {
        curSelected = Vector2Int.zero;
    }

    public void SelectTowards(Vector2 direction)
    {
        var newSelect = curSelected + direction.normalized;
        Select((int)newSelect.x, (int)newSelect.y);
    }

    public void Select(int x, int y)
    {
        var newSelect = new Vector2Int(x, y);
        OnSelectChange?.Invoke((curSelected, newSelect));
        curSelected = newSelect;
    }
}
