using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : BasePanelUI
{
    [SerializeField] private GridLayoutGroup layout;
    [SerializeField] private BaseGrid<ItemSO> itemGrid;
    [SerializeField] private InventorySlotUI slotUI;

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
    }

    private void SetupInventory(InventorySO inventorySO)
    {
        inventory = inventorySO;

        // Setup grid
        itemGrid = new BaseGrid<ItemSO>(2, 4, 1);
        for (int i = 0; i < itemGrid.Width * itemGrid.Height; i++)
        {
            // Get slot UI and item if any
            var newSlot = Instantiate(slotUI, layout.transform);
            slots.Add(newSlot);
        }

        Refresh();
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
            slots[s].Setup(itemSlot);

            // Set the item on the grid
            int x = s % itemGrid.Width;
            int y = s / itemGrid.Width;
            itemGrid.SetValue(x, y, itemSlot);
        }
    }
}
