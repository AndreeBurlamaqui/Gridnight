using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NexusPanelUI : BasePanelUI
{
    [SerializeField] private VerticalLayoutGroup layout;
    [SerializeField] private InventorySlotUI slotUIPrefab;
    [SerializeField] private InventorySO inventory;
    [SerializeField] private InputReader playerInput;
    [SerializeField] private InventorySlotUI feedSlot;

    private List<InventorySlotUI> slots = new();

    private void OnEnable()
    {
        inventory.Open(); // Open inventory to show what the player has
        feedSlot.Setup(null);
        playerInput.ChangeType(InputReader.MapType.UI);

        playerInput.OnNavigate += MoveInsideInventory;
        playerInput.OnInteract += TryInteractOnSlot;
    }

    private void OnDisable()
    {
        playerInput.OnNavigate -= MoveInsideInventory;
        playerInput.OnInteract -= TryInteractOnSlot;
    }

    private void MoveInsideInventory(Vector2Int dir)
    {
        if (!inventory.TryGetPanel(out InventoryPanelUI inventoryPanel))
        {
            return;
        }

        if (inventoryPanel.ItemGrid.CurrentSelected.x <= 0)
        {
            // If it's on the right, check if the player is trying to feed the nexus
            if (dir.x < 0)
            {
                PickManager.Instance.MovePick(feedSlot.transform.position);
                feedSlot.SetSelect(true);
                inventoryPanel.GetSelectedSlot().SetSelect(false);
                return;
            }
        }


        if (feedSlot.IsSelected)
        {
            // Go back to the old slot, without moving
            var selectedSlot = inventoryPanel.GetSelectedSlot();
            PickManager.Instance.MovePick(selectedSlot.transform.position);
            selectedSlot.SetSelect(true);
        }
        else
        {
            // Normal navigation
            inventoryPanel.NavigateSelection(dir);
        }
        feedSlot.SetSelect(false);
    }

    private void TryInteractOnSlot()
    {
        if (!inventory.TryGetPanel(out InventoryPanelUI inventoryPanel))
        {
            return;
        }

        if (feedSlot.IsSelected)
        {
            // Feed the nexus
        }
        else
        {
            // Try picking on inventory
            inventoryPanel.PickSelection();
        }
    }
}
