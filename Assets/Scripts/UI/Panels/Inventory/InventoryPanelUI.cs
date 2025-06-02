using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelUI : BasePanelUI
{
    [SerializeField] private GridLayoutGroup layout;
    [SerializeField] private InventorySlotUI slotUI;

    [SerializeField] private TMP_Text selectedTitleLabel;
    [SerializeField] private TMP_Text selectedDescriptionLabel;
    [SerializeField] private Image foodIcon;
    [SerializeField] private TMP_Text foodLabel;

    private InventorySO inventory;
    private List<InventorySlotUI> slots = new();
    private Vector2Int fromPickSelect;

    public SelectGrid<ItemSO> ItemGrid { get; private set; }

    protected override void Initiate()
    {
        base.Initiate();
        if(panelSO is InventorySO inventorySO)
        {
            SetupInventory(inventorySO);
            inventorySO.OnItemRemove.AddListener(OnItemRemoved);
        }
    }

    private void OnEnable()
    {
        if (ItemGrid != null)
        {
            ItemGrid.Select(0, 0);
            UpdateInventory();
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (PickManager.Instance != null)
        {
            PickManager.Instance.Drop();
        }
    }

    private void SetupInventory(InventorySO inventorySO)
    {
        inventory = inventorySO;

        // Setup grid
        ItemGrid = new(2, 4);
        ItemGrid.OnSelectChange += UpdateSlotSelect;
        for (int i = 0; i < ItemGrid.Width * ItemGrid.Height; i++)
        {
            // Get slot UI and item if any
            var newSlot = Instantiate(slotUI, layout.transform);
            slots.Add(newSlot);

            var itemSlot = i < inventory.ItemCount ? inventory.GetItemFromIndex(i) : null;

            // Set the item on the grid
            (int x, int y) = ItemGrid.IndexToGrid(i, layout);
            ItemGrid.SetValue(x, y, itemSlot);
            newSlot.gameObject.name = $"Slot [{x},{y}]";
        }

        // Select first option and refresh
        slots[ItemGrid.SelectedToIndex(layout)].SetSelect(true);
        UpdateInventory();
        Refresh();
    }

    public void UpdateInventory()
    {
        for(int i = 0; i < inventory.ItemCount; i++)
        {
            var item = inventory.GetItemFromIndex(i);
            if (item.TotalAmount <= 0)
            {
                // Should not be inventory
                inventory.RemoveItem(item);
                continue;
            }

            // If the item has a slot saved, add it to that
            // Otherwise, find a free slot
            int itemXPos = -1;
            int itemYPos = -1;
            if (item.HasSavedSlotPosition())
            {
                itemXPos = item.SlotGridPosition.x;
                itemYPos = item.SlotGridPosition.y;
            }
            else
            {
                (itemXPos, itemYPos) = ItemGrid.FindFreePosition();
            }

            if(itemXPos < 0 || itemYPos < 0)
            {
                continue; // Invalid position
            }

            item.UpdateSlotPosition(itemXPos, itemYPos);
            ItemGrid.SetValue(itemXPos, itemYPos, item);
        }
    }

    private void UpdateSlotSelect((Vector2Int oldSelect, Vector2Int newSelect) selectUpdate)
    {
        var oldIndex = ItemGrid.GridToIndex(selectUpdate.oldSelect.x, selectUpdate.oldSelect.y, layout);
        slots[oldIndex].SetSelect(false);
        var newIndex = ItemGrid.GridToIndex(selectUpdate.newSelect.x, selectUpdate.newSelect.y, layout);
        slots[newIndex].SetSelect(true);
    }

    public void Refresh()
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
            var (x, y) = ItemGrid.IndexToGrid(s, layout);
            if (ItemGrid.TryGetValue(x, y, out var itemSlot))
            {
                slot.Setup(itemSlot);
            }
            else
            {
                slot.Setup(null);
            }
        }

        bool showItemInfo = false;
        if (ItemGrid.TryGetSelectedValue(out var selectedItem))
        {
            selectedTitleLabel.text = selectedItem.Title;
            selectedDescriptionLabel.text = selectedItem.Description;

            var (foodItem, foodAmount) = selectedItem.GetFood();
            foodIcon.sprite = foodItem.Icon;
            foodIcon.preserveAspect = true;
            foodLabel.text = "x" + foodAmount;
            showItemInfo = true;
        }

        selectedTitleLabel.gameObject.SetActive(showItemInfo);
        selectedDescriptionLabel.gameObject.SetActive(showItemInfo);
        foodIcon.gameObject.SetActive(showItemInfo);
        foodLabel.gameObject.SetActive(showItemInfo);
    }

    public void NavigateSelection(Vector2Int direction)
    {
        if (ItemGrid == null || inventory == null)
        {
            return;
        }

        ItemGrid.SelectTowards(direction);
        if (PickManager.Instance.IsPicking)
        {
            var selectedSlot = GetSelectedSlot();
            PickManager.Instance.MovePick(selectedSlot.transform.position);
        }
    }

    public InventorySlotUI GetSelectedSlot()
    {
        return slots[ItemGrid.SelectedToIndex(layout)];
    }

    public void PickSelection()
    {
        if(ItemGrid == null)
        {
            return;
        }

        if (PickManager.Instance.IsPicking)
        {
            PickManager.Instance.Drop();
            if (!TryGetPickedItem(out var fromSlot))
            {
                return; // Nothing on origin (shouldn't happen)
            }

            if (ItemGrid.TryGetSelectedValue(out var selectedSlot))
            {
                // If something in the slot, then we need to swap
                Debug.Log("Droping on slot filled. Swapping");
                ItemGrid.SetValue(fromPickSelect.x, fromPickSelect.y, selectedSlot);
                selectedSlot.UpdateSlotPosition(fromPickSelect.x, fromPickSelect.y);
            }
            else
            {
                // Nothing on slot, just set it
                Debug.Log("Droping on slot empty. Setting");
                ItemGrid.SetValue(fromPickSelect.x, fromPickSelect.y, null);
            }

            fromSlot.UpdateSlotPosition(ItemGrid.CurrentSelected.x, ItemGrid.CurrentSelected.y);
            ItemGrid.SetSelectedValue(fromSlot);
            Refresh();
        }
        else
        {
            if (!ItemGrid.IsSelectedFilled())
            {
                return; // Nothing to pick
            }

            var pickedSlot = slots[ItemGrid.SelectedToIndex(layout)];
            PickManager.Instance.OnPick(pickedSlot.transform as RectTransform);
            pickedSlot.SetDisplay(false);
            fromPickSelect = ItemGrid.CurrentSelected;
            Debug.Log("Picking " + pickedSlot.gameObject.name);
        }
    }

    public void Deselect()
    {
        if (!PickManager.Instance.IsPicking)
        {
            return;
        }

        ItemGrid.Select(fromPickSelect.x, fromPickSelect.y);
        PickManager.Instance.Drop();
        Refresh();
    }

    public bool TryGetPickedItem(out ItemSO pickedItem)
    {
        pickedItem = null;
        if (!PickManager.Instance.IsPicking)
        {
            return false;
        }

        if (!ItemGrid.TryGetValue(fromPickSelect.x, fromPickSelect.y, out pickedItem))
        {
            return false; // Nothing on origin (shouldn't happen)
        }

        return true;
    }

    public void OnItemRemoved(ItemSO itemRemoved)
    {
        if(!itemRemoved.HasSavedSlotPosition())
        {
            // Item wasn't on grid yet
            return;
        }

        Debug.Log($"Item {itemRemoved.Title} was removed from inventory, removing from grid position {itemRemoved.SlotGridPosition}");
        ItemGrid.SetValue(itemRemoved.SlotGridPosition.x, itemRemoved.SlotGridPosition.y, null);
        itemRemoved.UpdateSlotPosition(-1, -1); // Reset slot position
    }
}
