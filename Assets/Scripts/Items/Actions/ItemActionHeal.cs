using UnityEngine;

[CreateAssetMenu(fileName = "ItemActionHeal", menuName = "Scriptable Objects/Item Actions/Heal")]
public class ItemActionHeal : ItemAction
{
    [SerializeField] private int amount;
    public override void Execute(ItemSO item)
    {
        WaveManager.Instance.HealNexus(amount);
        item.Add(-1); // Consume
    }
}
