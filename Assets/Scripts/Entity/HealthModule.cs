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
    /// <summary>
    /// Callback the range of Current/Start HP
    /// </summary>
    public UnityEvent<float> OnUpdate;

    private string DamageTweenID => "DAMAGE_TWEEN_" + GetInstanceID();
    private string HealTweenID => "HEAL_TWEEN_" + GetInstanceID();

    public override void Initiate(BaseEntity controller)
    {
        base.Initiate(controller);
        CurrentHP = StartHP;
    }

    public void DamageBy(int dmg)
    {
        if (CurrentHP <= 0)
        {
            Kill();
            return;
        }

        DOTween.Kill(DamageTweenID, true);
        var oldRot = transform.rotation;
        CurrentHP -= dmg;
        OnUpdate?.Invoke((float)CurrentHP / (float)StartHP);

        transform.DOShakeRotation(0.15f, 55, 25, randomnessMode: ShakeRandomnessMode.Harmonic)
            .OnComplete(OnFinishTween)
            .SetId(DamageTweenID);

        void OnFinishTween(){
            transform.rotation = oldRot;
            if (CurrentHP <= 0)
            {
                Kill();
            }
        }
    }

    public void HealBy(int amount)
    {
        if (!IsAlive)
        {
            return; // Can't heal what's already dead
        }

        CurrentHP = Math.Min(CurrentHP + amount, CurrentHP);
        OnUpdate?.Invoke((float)CurrentHP / (float)StartHP);
        DOTween.Kill(HealTweenID, true);
        var oldScale = transform.localScale;
        transform.DOPunchScale(Vector3.one, 0.25f)
        .OnComplete(OnFinishTween)
        .SetId(HealTweenID);

        CurrentHP += amount;
        void OnFinishTween()
        {
            transform.localScale = oldScale;
        }
    }

    public void Kill()
    {
        CurrentHP = 0; // Ensure it's dead
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}
