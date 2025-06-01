using DG.Tweening;
using System;
using UnityEngine;

public class MovementModule : EntityModule
{
    [field: SerializeField] public float JumpCooldown { get; private set; }

    [Header("RUNTIME")]
    [field: SerializeField] public bool CanJump { get; private set; }

    public override void Initiate(BaseEntity controller)
    {
        base.Initiate(controller);
        CanJump = true;
    }

    public void Move(Vector2 newPos)
    {
        if (!WorldGrid.Instance.IsInitiated)
        {
            return;
        }

        if (!CanJump)
        {
            //Debug.Log($"Entity {gameObject.name} can't jump");
            return;
        }

        Debug.Log($"Moving entity {gameObject.name} to {newPos}");
        transform.DOKill();
        transform.DOJump(newPos, 0.5f, 1, 0.15f);

        // Block jump to not spamm
        DOVirtual.DelayedCall(JumpCooldown, EnableJump);
        CanJump = false;
    }

    private void EnableJump()
    {
        CanJump = true;
    }
}
