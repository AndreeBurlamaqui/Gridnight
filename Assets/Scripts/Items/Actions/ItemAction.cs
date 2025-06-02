using UnityEngine;

public abstract class ItemAction : ScriptableObject
{
    [field: SerializeField] public string Title { get; private set; }

    public abstract void Execute(ItemSO item);
}
