using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountLabel;

    [SerializeField] private Image selectHighlight;

    public bool IsSelected { get; private set; }

    public void Setup(ItemSO item)
    {
        if (item != null)
        {
            icon.sprite = item.Icon;
            amountLabel.text = item.TotalAmount.ToString("00");
        }

        icon.gameObject.SetActive(item != null);
        amountLabel.gameObject.SetActive(item != null);
        SetDisplay(true);
        selectHighlight.color = selectHighlight.color.SetAlpha(IsSelected ? 1 : 0);
    }

    public void SetSelect(bool state)
    {
        IsSelected = state;
        //Debug.Log($"Setting slot {gameObject.name} {(IsSelected ? "selected" : "deselected")}");
        if (IsSelected)
        {
            selectHighlight.transform.DOPunchScale(Vector2.one * 0.5f, 0.25f).OnComplete(ResetHighlightScale);
            selectHighlight.DOFade(1, 0.15f);
        }
        else
        {
            selectHighlight.DOFade(0, 0.1f);
        }
    }

    private void ResetHighlightScale()
    {
        selectHighlight.transform.localScale = Vector3.one;
    }

    public void SetDisplay(bool state)
    {
        int fadeAlpha = state ? 1 : 0;
        amountLabel.DOFade(fadeAlpha, 0.1f);
        icon.DOFade(fadeAlpha, 0.1f);
    }
}
