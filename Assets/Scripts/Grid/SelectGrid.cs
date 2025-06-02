using System;
using UnityEngine;

public class SelectGrid<T> : BaseGrid<T>
{
    public Vector2Int CurrentSelected { get; private set; }

    public event Action<(Vector2Int oldSelect, Vector2Int newSelect)> OnSelectChange;

    public SelectGrid(int _width, int _height) : base(_width, _height, 1)
    {
        CurrentSelected = Vector2Int.zero;
    }

    public void SelectTowards(Vector2 direction)
    {
        int newX = Mathf.Clamp(CurrentSelected.x + (int)direction.x, 0, Width - 1);
        int newY = Mathf.Clamp(CurrentSelected.y + ((int)direction.y * -1), 0, Height - 1);

        Select(newX, newY);
    }

    public void Select(int x, int y)
    {
        var newSelect = new Vector2Int(x, y);
        if (newSelect == CurrentSelected)
        {
            return;
        }

        //Debug.Log($"Selecting grid " + newSelect);
        OnSelectChange?.Invoke((CurrentSelected, newSelect));
        CurrentSelected = newSelect;
    }

    public int SelectedToIndex(UnityEngine.UI.GridLayoutGroup layout) => GridToIndex(CurrentSelected.x, CurrentSelected.y, layout);
    public bool IsSelectedFilled() => IsPositionFilled(CurrentSelected.x, CurrentSelected.y);
    public bool TryGetSelectedValue(out T value) => TryGetValue(CurrentSelected.x, CurrentSelected.y, out value);
    public void SetSelectedValue(T value) => SetValue(CurrentSelected.x, CurrentSelected.y, value);
}
