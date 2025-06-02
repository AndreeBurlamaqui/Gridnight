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

        playerInput.OnNavigate.AddListener(MoveInsideInventory);
        playerInput.OnInteract.AddListener(TryInteractOnSlot);

        UpdateRequirements();
    }

    private void OnDisable()
    {
        playerInput.OnNavigate.RemoveListener(MoveInsideInventory);
        playerInput.OnInteract.RemoveListener(TryInteractOnSlot);
    }
    
    private void UpdateRequirements()
    {
        if (slots.Count < WaveManager.Instance.MaximumPossibleFoods)
        {
            // Create the slots
            Debug.Log("Creating requirement nexus slots");
            for (int f = 0; f < WaveManager.Instance.MaximumPossibleFoods; f++)
            {
                slots.Add(Instantiate(slotUIPrefab, layout.transform));
            }
        }

        for (int s = 0; s < slots.Count; s++)
        {
            // Ensure we won't have more than needed
            slots[s].gameObject.SetActive(false);
        }

        int slotIndex = 0;
        foreach (var requirement in WaveManager.Instance.LoopWaveRequirements())
        {
            var slot = slots[slotIndex];

            int amountShown = requirement.amountRequired - requirement.amountAchieved;
            slot.Setup(requirement.item, amountShown);
            slot.SetInteractable(amountShown <= 0);
            slot.gameObject.SetActive(true);

            slotIndex++;
        }
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

        if (feedSlot.IsSelected && inventoryPanel.TryGetPickedItem(out var selectedItem))
        {
            // Check if selected item is a requirement
            var food = selectedItem.GetFood();
            if (WaveManager.Instance.IsRequirement(food.item, out int requiredAmount) && requiredAmount > 0)
            {
                // If so, Feed the nexus
                int amountToGive = Mathf.Min(requiredAmount, food.amount);
                int bouldersToRemove = Mathf.CeilToInt((float)amountToGive / food.amount);

                WaveManager.Instance.AddRequirementAchieveAmount(food.item, bouldersToRemove * food.amount);
                Debug.Log($"Fed the nexus with {selectedItem.Title} x{bouldersToRemove} (equivalent to {bouldersToRemove * food.amount} units)"); 
                
                // Then reduce quantity
                selectedItem.Add(-bouldersToRemove);
                if(selectedItem.TotalAmount <= 0)
                {
                    // Remove from inventory
                    inventory.RemoveItem(selectedItem);
                }

                inventoryPanel.UpdateInventory();
                inventoryPanel.Refresh();
                UpdateRequirements();
                PickManager.Instance.Drop();
            }
            else
            {
                // Otherwise cancel operation and go back to inventory
                inventoryPanel.Deselect();
                Debug.Log("Not a nexus requirement. Deselecting");
            }
        }
        else
        {
            // Try picking on inventory
            inventoryPanel.PickSelection();
        }
    }
}
