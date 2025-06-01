using UnityEngine;

public abstract class BasePanelUI : MonoBehaviour
{
    [SerializeField] protected PanelUISO panelSO;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        panelSO.AssignPanel(this);
        SetState(false);
    }

    public void SetState(bool isActive)
    {
        IsOpen = isActive;
        gameObject.SetActive(IsOpen);
    }
}
