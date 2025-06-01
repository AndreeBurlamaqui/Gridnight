using DG.Tweening;
using UnityEngine;

public class PickManager : MonoBehaviour
{
    #region SINGLETON

    public static PickManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    private RectTransform pickClone;
    public bool IsPicking => pickClone != null;

    public void OnPick(RectTransform original)
    {
        pickClone = Instantiate(original, transform);
        pickClone.position = original.position;
        pickClone.DOSizeDelta(original.sizeDelta, 0.15f);
    }

    public void MovePick(Vector3 position)
    {
        if(pickClone == null)
        {
            return;
        }

        pickClone.DOMove(position, 0.15f).SetEase(Ease.InOutBack);
    }

    public void Drop()
    {
        Destroy(pickClone.gameObject);
    }
}
