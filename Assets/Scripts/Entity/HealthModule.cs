using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthModule : EntityModule
{
    [field: SerializeField] public int StartHP { get; private set; }

    [field: Header("RUNTIME")]
    [field: SerializeField] public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;

    public UnityEvent<HealthModule> OnDeath;

    private string DamageTweenID => "DAMAGE_TWEEN_" + GetInstanceID();

    public override void Initiate(BaseEntity controller)
    {
        base.Initiate(controller);
        CurrentHP = StartHP;
    }

    public void DamageBy(int dmg)
    {
        DOTween.Kill(DamageTweenID);
        transform.DOShakeRotation(0.15f, 55, 25, randomnessMode: ShakeRandomnessMode.Harmonic)
            .OnComplete(OnFinishTween)
            .SetId(DamageTweenID);

        void OnFinishTween(){
            CurrentHP -= dmg;

            if (CurrentHP <= 0)
            {
                Kill();
            }
        }
    }

    private void Kill()
    {
        CurrentHP = 0; // Ensure it's dead
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
