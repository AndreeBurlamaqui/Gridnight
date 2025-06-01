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

    private InventorySO inventory;
    private List<InventorySlotUI> slots = new();
    private Vector2Int fromPickSelect;

    private void Start()
    {
        if(panelSO is InventorySO inventorySO)
        {
            SetupInventory(inventorySO);
        }
    }

    private void OnEnable()
    {
        if (itemGrid != null)
        {
            itemGrid.Select(0, 0);
            Refresh();
        }

        playerInput.OnNavigate += NavigateSelection;
        playerInput.OnInteract += PickSelection;
    }

    private void OnDisable()
    {
        playerInput.OnNavigate -= NavigateSelection;
        playerInput.OnInteract -= PickSelection;
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

            var itemSlot = i < inventory.items.Count ? inventory.items[i] : null;

            // Set the item on the grid
            (int x, int y) = itemGrid.IndexToGrid(i, layout);
            itemGrid.SetValue(x, y, itemSlot);
            newSlot.gameObject.name = $"Slot [{x},{y}]";
        }

        // Select first option and refresh
        slots[itemGrid.SelectedToIndex(layout)].SetSelect(true);
        Refresh();
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

        // Loop grid to setup the slots
        for (int s = 0; s < slots.Count; s++)
        {
            // Set it to the slot UI
            var slot = slots[s];
            var gridIndex = itemGrid.IndexToGrid(s, layout);
            if (itemGrid.TryGetValue(gridIndex.x, gridIndex.y, out var itemSlot))
            {
                slot.Setup(itemSlot);
            }
            else
            {
                slot.Setup(null);
            }
        }
    }

    private void NavigateSelection(Vector2Int direction)
    {
        if (itemGrid == null || inventory == null)
        {
            return;
        }

        itemGrid.SelectTowards(direction);
        if (PickManager.Instance.IsPicking)
        {
            var selectedSlot = slots[itemGrid.SelectedToIndex(layout)];
            PickManager.Instance.MovePick(selectedSlot.transform.position);
        }
    }

    private void PickSelection()
    {
        if (PickManager.Instance.IsPicking)
        {
            PickManager.Instance.Drop();
            if (!itemGrid.TryGetValue(fromPickSelect.x, fromPickSelect.y, out var fromSlot))
            {
                return; // Nothing on origin (shouldn't happen)
            }

            if (itemGrid.TryGetSelectedValue(out var selectedSlot))
            {
                // If something in the slot, then we need to swap
                Debug.Log("Droping on slot filled. Swapping");
            }
            else
            {
                // Nothing on slot, just set it
                Debug.Log("Droping on slot empty. Setting");
                itemGrid.SetValue(fromPickSelect.x, fromPickSelect.y, null);
                itemGrid.SetSelectedValue(fromSlot);
                Refresh();
            }
        }
        else
        {
            if (!itemGrid.IsSelectedFilled())
            {
                return; // Nothing to pick
            }

            var pickedSlot = slots[itemGrid.SelectedToIndex(layout)];
            PickManager.Instance.OnPick(pickedSlot.transform as RectTransform);
            pickedSlot.SetDisplay(false);
            fromPickSelect = itemGrid.CurrentSelected;
            Debug.Log("Picking " + pickedSlot.gameObject.name);
        }
    }
}
