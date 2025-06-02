using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/Panels/Inventory")]
public class InventorySO : PanelUISO
{
    [Header("RUNTIME")]
    [SerializeField] private List<ItemSO> items = new();

    [Header("SAVING")]
    public ItemSO[] everyItemInProject;

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

    [ContextMenu("Update Every Item List")]
    private void GetEveryItemInDatabase() {
        string[] guids = AssetDatabase.FindAssets("t:ItemSO");
        List<ItemSO> allItems = new List<ItemSO>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
            if (item != null)
            {
                allItems.Add(item);
            }
        }

        everyItemInProject = allItems.ToArray();
        Debug.Log($"Updated item list: {everyItemInProject.Length} items found.");
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

    public void Load(List<SavedInventoryItem> savedItems)
    {
        for (int i = 0; i < savedItems.Count; i++)
        {
            SavedInventoryItem savedItem = savedItems[i];
            ItemSO matchingItem = null;

            // Look for matching ItemSO by ID
            for (int e = 0; e < everyItemInProject.Length; e++)
            {
                ItemSO itemSO = everyItemInProject[e];
                if (itemSO.name == savedItem.itemID)
                {
                    matchingItem = itemSO;
                    break;
                }
            }

            if (matchingItem != null)
            {
                matchingItem.Add(savedItem.quantity); // Add quantity
                AddItem(matchingItem); // Add to inventory
                Debug.Log($"Loaded item: {matchingItem.Title} x{savedItem.quantity}");
            }
            else
            {
                Debug.LogWarning($"Item with ID {savedItem.itemID} not found in project.");
            }
        }
    }
}
