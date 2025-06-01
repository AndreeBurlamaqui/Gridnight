using DG.Tweening;
using System;
using UnityEngine;

public class MovementModule : EntityModule
{
    [field: SerializeField] public float JumpCooldown { get; private set; }

    [Header("RUNTIME")]
    [field: SerializeField] public bool CanJump { get; private set; }

    private Sequence freeMoveSequence;
    private Sequence hitMoveSequence;

    public override void Initiate(BaseEntity controller)
    {
        base.Initiate(controller);
        CanJump = true;
    }

    public void FreeMove(Vector2 newPos)
    {
        if (!CanJump)
        {
            //Debug.Log($"Entity {gameObject.name} can't jump");
            return;
        }

        //Debug.Log($"Moving entity {gameObject.name} to {newPos}");
        freeMoveSequence.Kill();
        freeMoveSequence.Append(transform.DOJump(newPos, 0.5f, 1, 0.15f));
        freeMoveSequence.Append(DOVirtual.DelayedCall(JumpCooldown, EnableJump)); // Block jump to not spamm
        
        CanJump = false;
    }

    public void HitMove(Vector2 hitPos)
    {
        if (!CanJump)
        {
            //Debug.Log($"Entity {gameObject.name} can't jump");
            return;
        }

        //Debug.Log($"Moving entity {gameObject.name} to {hitPos}");
        hitMoveSequence.Kill();
        hitMoveSequence = DOTween.Sequence();
        var originalPos = transform.position;
        hitMoveSequence.Append(transform.DOJump(hitPos, 0.25f, 1, 0.15f));
        hitMoveSequence.Append(transform.DOJump(originalPos, 0.25f, 1, 0.15f));
        hitMoveSequence.Append(DOVirtual.DelayedCall(JumpCooldown, EnableJump)); // Block jump to not spamm
        
        CanJump = false;
    }

    private void EnableJump()
    {
        CanJump = true;
    }
}
