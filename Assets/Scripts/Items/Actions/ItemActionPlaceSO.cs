using UnityEngine;

[CreateAssetMenu(fileName = "ItemActionPlaceSO", menuName = "Scriptable Objects/Item Actions/Place")]
public class ItemActionPlaceSO : ItemAction
{
    [SerializeField] private InputReader playerInput;
    public override void Execute(ItemSO item)
    {
        // Start building type
    }
}
