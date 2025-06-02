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

        playerInput.OnNavigate += CheckNavigationAttempt;
    }

    private void OnDisable()
    {
        playerInput.OnNavigate -= CheckNavigationAttempt;
    }

    private void CheckNavigationAttempt(Vector2Int dir)
    {
        if(dir.x >= 0)
        {
            return;
        }

        // If it's on the right, check if the player is trying to feed the nexus
        if(!inventory.TryGetPanel(out InventoryPanelUI inventoryPanel))
        {
            return;
        }

        if (inventoryPanel.ItemGrid.CurrentSelected.x <= 0)
        {
            PickManager.Instance.MovePick(feedSlot.transform.position);
        }
    }
}
