using UnityEngine;

public class HealthModule : EntityModule
{
    [field: SerializeField] public int StartHP { get; private set; }

    [field: Header("RUNTIME")]
    [field: SerializeField] public int CurrentHP { get; private set; }

    public override void Initiate(BaseEntity controller)
    {
        base.Initiate(controller);
        CurrentHP = StartHP;
    }
}
