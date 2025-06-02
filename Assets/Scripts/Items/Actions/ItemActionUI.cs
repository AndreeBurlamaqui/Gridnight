using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemActionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleLabel;
    private Button bttn;
    private Action curAction;

    private void Awake()
    {
        bttn = gameObject.GetOrAddComponent<Button>();    
    }

    public void Setup(string title, Action onClickAction)
    {
        titleLabel.text = title;

        bttn.onClick.RemoveAllListeners();
        bttn.onClick.AddListener(OnClick);

        curAction = onClickAction;
    }

    public void Select()
    {
        bttn.Select();
    }

    private void OnClick()
    {
        curAction?.Invoke();
    }
}
