using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : BasePanelUI
{
    [SerializeField] private GridLayoutGroup layout;
    [SerializeField] private SelectGrid<ItemSO> itemGrid;
    [SerializeField] private InventorySlotUI slotUI;
    [SerializeField] private InputReader playerInput;

    InventorySO inventory;
    List<InventorySlotUI> slots = new();

    private void Start()
    {
        if(panelSO is InventorySO inventorySO)
        {
            SetupInventory(inventorySO);
        }
    }

    private void OnEnable()
    {
        Refresh();

        if(itemGrid != null)
        {
            itemGrid.Select(0, 0);
        }

        playerInput.OnNavigate += NavigateSelection;
    }

    private void OnDisable()
    {
        playerInput.OnNavigate -= NavigateSelection;
    }

    private void SetupInventory(InventorySO inventorySO)
    {
        inventory = inventorySO;

        // Setup grid
        itemGrid = new(2, 4);
        itemGrid.OnSelectChange += UpdateSlotSelect;
        for (int i = 0; i < itemGrid.Width * itemGrid.Height; i++)
        {
            // Get slot UI and item if any
            var newSlot = Instantiate(slotUI, layout.transform);
            slots.Add(newSlot);
        }

        // Refresh and select first option
        Refresh();
        itemGrid.Select(0, 0);
    }

    private void UpdateSlotSelect((Vector2Int oldSelect, Vector2Int newSelect) selectUpdate)
    {
        var oldIndex = itemGrid.GridToIndex(selectUpdate.oldSelect.x, selectUpdate.oldSelect.y, layout);
        slots[oldIndex].SetSelect(false);
        var newIndex = itemGrid.GridToIndex(selectUpdate.newSelect.x, selectUpdate.newSelect.y, layout);
        slots[newIndex].SetSelect(true);
    }

    private void Refresh()
    {
        if(inventory == null)
        {
            return;
        }

        for (int s = 0; s < slots.Count; s++)
        {
            var itemSlot = s < inventory.items.Count ? inventory.items[s] : null;

            // Set it to the slot UI
            var slot = slots[s];
            slot.Setup(itemSlot);

            // Set the item on the grid
            (int x, int y) = itemGrid.IndexToGrid(s, layout);
            itemGrid.SetValue(x, y, itemSlot);
            slot.gameObject.name = $"Slot [{x},{y}]";
        }
    }

    private void NavigateSelection(Vector2Int direction)
    {
        if(itemGrid == null || inventory == null)
        {
            return;
        }

        itemGrid.SelectTowards(direction);
    }
}
