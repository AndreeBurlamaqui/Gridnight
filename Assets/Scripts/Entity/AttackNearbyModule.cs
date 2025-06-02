using DG.Tweening;
using System;
using UnityEngine;

public class AttackNearbyModule : EntityModule
{
    [SerializeField] private string targetTag;
    [SerializeField] private float cooldownTimer = 0.5f;
    private bool canAttack, isOnCooldown;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!canAttack || isOnCooldown)
        {
            return;
        }

        if (collision.CompareTag(targetTag) && collision.TryGetComponent(out BaseEntity targetEntity))
        {
            Entity.Hit(targetEntity);
            isOnCooldown = true;

            DOVirtual.DelayedCall(cooldownTimer, FinishCooldown);
        }
    }

    private void FinishCooldown()
    {
        isOnCooldown = false;
    }

    public void SetAbility(bool state)
    {
        canAttack = state;
    }
}
