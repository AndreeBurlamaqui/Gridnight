using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/Panels/Trade")]
public class TradeSO : PanelUISO
{
    public TradeRecipe[] recipes;
}

[System.Serializable]
public class TradeRecipe
{
    public ItemSO itemRequire;
    public int amountRequire;

    public ItemSO result;
}
