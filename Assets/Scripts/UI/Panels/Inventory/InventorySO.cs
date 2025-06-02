using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/Panels/Inventory")]
public class InventorySO : PanelUISO
{
    [Header("RUNTIME")]
    [SerializeField] private List<ItemSO> items = new();

    public int ItemCount => items.Count;

    public UnityEvent OnInventoryChange;
    public UnityEvent<ItemSO> OnItemRemove;
    public UnityEvent<ItemSO> OnItemAdd;

#if UNITY_EDITOR

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetData();
        }
    }

#endif

    public void ResetData()
    {
        items.Clear();
    }

    public void AddItem(ItemSO newItem)
    {
        if (items.Contains(newItem))
        {
            return; // Don't duplicate
        }

        Debug.Log("Adding item to inventory " + newItem);
        items.Add(newItem);
        OnItemAdd?.Invoke(newItem);
        OnInventoryChange?.Invoke();
    }

    public void RemoveItem(ItemSO oldItem)
    {
        if (!items.Contains(oldItem))
        {
            return; // Not on this inventory
        }

        Debug.Log("Removing item from inventory " + oldItem);
        items.Remove(oldItem);
        OnItemRemove?.Invoke(oldItem);
        OnInventoryChange?.Invoke();
    }

    public ItemSO GetItemFromIndex(int index) => items[index];
}
