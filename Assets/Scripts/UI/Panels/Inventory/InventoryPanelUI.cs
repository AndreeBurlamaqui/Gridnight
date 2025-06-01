using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : BasePanelUI
{
    [SerializeField] private GridLayoutGroup layout;
    [SerializeField] private BaseGrid<ItemSO> itemGrid;
    [SerializeField] private InventorySlotUI slotUI;

    private void Start()
    {
        if(panelSO is InventorySO inventorySO)
        {
            SetupInventory(inventorySO);
        }
    }

    private void SetupInventory(InventorySO inventorySO)
    {
        // Setup grid
        itemGrid = new BaseGrid<ItemSO>(2, 4, 1);
        for (int i = 0; i < itemGrid.Width * itemGrid.Height; i++)
        {
            var newSlot = Instantiate(slotUI, layout.transform);
            var itemSlot = i < inventorySO.items.Count ? inventorySO.items[i] : null;
            newSlot.Setup(itemSlot);
        }

        Refresh(inventorySO);
    }


    private void Refresh(InventorySO inventorySO)
    {

    
    }
}
