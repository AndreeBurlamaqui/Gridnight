using UnityEngine;
using UnityEngine.UI;

public class TradeRecipeUI : MonoBehaviour
{
    [SerializeField] private InventorySlotUI ingredient;
    [SerializeField] private InventorySlotUI requirement;
    [SerializeField] private InventorySlotUI result;

    private TradeRecipe curRecipe;

    public RecipeState State { get; private set; }
    public enum RecipeState
    {
        WAITING_INGREDIENT,
        INGREDIENT_FULFILLED,
        NOT_ENOUGH_INGREDIENT,
    }

    public void Setup(TradeRecipe recipe)
    {
        curRecipe = recipe;
        ingredient.Setup(null);

        requirement.Setup(recipe.itemRequire, recipe.amountRequire);

        result.Setup(recipe.result);
        result.SetInteractable(false);

        UpdateState(RecipeState.WAITING_INGREDIENT);
    }

    public bool DropItem(ItemSO holdingItem)
    {
        if(State != RecipeState.WAITING_INGREDIENT)
        {
            return false;
        }

        bool isSameItem = holdingItem == curRecipe.itemRequire;
        bool hasEnoughItem = holdingItem.TotalAmount >= curRecipe.amountRequire;

        if(isSameItem && hasEnoughItem)
        {
            ingredient.Setup(holdingItem);
            UpdateState(RecipeState.INGREDIENT_FULFILLED);
            return true;
        }
        else
        {
            UpdateState(RecipeState.NOT_ENOUGH_INGREDIENT);
            return false;
        }
    }

    private void UpdateState(RecipeState newState)
    {
        State = newState;

        switch (State)
        {
            case RecipeState.WAITING_INGREDIENT:
                ingredient.SetSelect(false);

                result.SetInteractable(false);
                result.SetSelect(false);
                break;

            case RecipeState.INGREDIENT_FULFILLED:
                ingredient.SetSelect(false);

                result.SetInteractable(true);
                result.SetSelect(true);
                break;

            case RecipeState.NOT_ENOUGH_INGREDIENT:
                ingredient.SetSelect(true);

                result.SetInteractable(false);
                result.SetSelect(false);
                break;
        }
    }

    public void Navigate(int xDir)
    {

    }
}
