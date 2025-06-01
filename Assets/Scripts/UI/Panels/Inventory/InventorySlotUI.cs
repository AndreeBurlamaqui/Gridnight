using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountLabel;

    public void Setup(ItemSO item)
    {
        if (item != null)
        {
            icon.sprite = item.Icon;
            amountLabel.text = item.TotalAmount.ToString("00");
        }

        icon.gameObject.SetActive(item != null);
        amountLabel.gameObject.SetActive(item != null);
    }
}
