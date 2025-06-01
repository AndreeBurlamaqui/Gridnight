using System;
using UnityEngine;

public class TraderManager : MonoBehaviour
{
    [SerializeField] private InventorySO playerInventory;
    [SerializeField] private InputReader playerInput;
    private TradeSO curTrade;
    private bool isOnPlayerSide = true;

    private void OnEnable()
    {
        playerInput.OnNavigate += Navigate;
    }

    private void OnDisable()
    {
        playerInput.OnNavigate -= Navigate;
    }

    public void StartTrade(TradeSO trade)
    {
        curTrade = trade;

        // Open both inventory and trade
        curTrade.Open();
        playerInventory.Open();

        // Start with the inventory selected
        playerInventory.SetFocus(true);
        curTrade.SetFocus(false);
        isOnPlayerSide = true;
    }

    private void Navigate(Vector2Int direction)
    {
        var curTradeUI = curTrade.GetPanel<TradeUI>();
        if (isOnPlayerSide)
        {
            // Check if it's trying to go right
            if (direction.x >= 0)
            {
                return;
            }

            // And if it's on the correct border
        }
        else
        {
            // Check if it's trying to change y
            if(direction.y != 0)
            {
                // Up or down on the recipes
                curTradeUI.Navigate(direction.y);
            }
            else
            {
                // If it's trying to change x

                // Check if which slot UI is selected on recipe
            
            }
        }
    }
}
