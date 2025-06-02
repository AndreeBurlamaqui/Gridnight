using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountLabel;
    [SerializeField] private Image selectHighlight;

    private CanvasGroup group;

    public bool IsSelected { get; private set; }
    public bool IsInteractable { get; private set; }

    public void Setup(Sprite iconSprite, int amount)
    {
        bool hasIcon = iconSprite != null;
        if (hasIcon)
        {
            icon.sprite = iconSprite;
        }

        bool hasAmount = amount > 0;
        if (hasAmount)
        {
            amountLabel.text = amount.ToString("00");
        }

        icon.gameObject.SetActive(hasIcon);
        amountLabel.gameObject.SetActive(hasAmount);
        SetDisplay(true);
        selectHighlight.color = selectHighlight.color.SetAlpha(IsSelected ? 1 : 0);

        group = gameObject.GetOrAddComponent<CanvasGroup>();
    }

    public void Setup(ItemSO item)
    {
        if (item == null)
        {
            Setup((Sprite)null, -1);
            return;
        }

        Setup(item.Icon, item.TotalAmount);
    }

    public void Setup(ItemSO item, int customAmount)
    {
        if (item == null)
        {
            Setup((Sprite)null, -1);
            return;
        }

        Setup(item.Icon, customAmount);
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

    public void SetInteractable(bool state)
    {
        IsInteractable = state;
        group.DOFade(IsInteractable ? 1 : 0.5f, 0.15f);
    }
}
