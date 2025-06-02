using DG.Tweening;
using System;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private InventorySO backpack;
    [SerializeField] private ItemSO item;
    [SerializeField] private int quantity;
    private bool collected = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        if (other.CompareTag(PlayerEntity.PLAYER_TAG))
        {
            collected = true;
            Debug.Log($"Player starting to collect {item.Title}({item.TotalAmount})");
            var moveSequence = DOTween.Sequence();
            var randomArc = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle;
            moveSequence.Append(transform.DOJump(randomArc, 2, 1, 0.15f));
            moveSequence.AppendCallback(SetAsChild);
            moveSequence.Append(transform.DOLocalMove(Vector2.zero, 0.15f));
            moveSequence.AppendCallback(OnItemCollected);
            void SetAsChild()
            {
                transform.SetParent(other.transform);
            }
        }
    }

    private void OnItemCollected()
    {
        backpack.items.Add(item);
        item.Add(quantity);
        Debug.Log($"Player collected {item.Title}({item.TotalAmount})");
        Destroy(gameObject);
    }
}
