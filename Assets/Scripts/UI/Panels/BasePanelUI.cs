using UnityEngine;

public abstract class BasePanelUI : MonoBehaviour
{
    [SerializeField] protected PanelUISO panelSO;

    public bool IsOpen { get; private set; }
    public bool IsInitiated { get; private set; }

    private void Awake()
    {
        if (!IsInitiated)
        {
            SetState(false);
        }

        EnsureInitiation();
    }

    private void EnsureInitiation()
    {
        if (!IsInitiated)
        {
            Initiate();
        }
    }

    protected virtual void Initiate()
    {
        panelSO.AssignPanel(this);
        IsInitiated = true;
    }

    public void SetState(bool isActive)
    {
        EnsureInitiation();
        IsOpen = isActive;
        gameObject.SetActive(IsOpen);
    }
}
