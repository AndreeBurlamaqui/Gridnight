using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeUI : BasePanelUI
{
    [SerializeField] private TradeRecipeUI recipeUI;
    [SerializeField] private VerticalLayoutGroup layout;

    private TradeSO trade;
    private List<TradeRecipeUI> recipes = new();
    private int selectedRecipe = 0;

    private void Start()
    {
        if (panelSO is TradeSO tradeSO)
        {
            SetupTrade(tradeSO);
        }
    }

    private void SetupTrade(TradeSO tradeSO)
    {
        trade = tradeSO;
        selectedRecipe = 0;

        for(int r = 0; r < trade.recipes.Length; r++)
        {
            var newRecipeUI = Instantiate(recipeUI, layout.transform);
            newRecipeUI.Setup(trade.recipes[r]);
            recipes.Add(newRecipeUI);
        }
    }

    public void Navigate(int yDirection)
    {
        selectedRecipe += yDirection;
        PickManager.Instance.MovePick(recipes[selectedRecipe].transform.position);
    }
}
