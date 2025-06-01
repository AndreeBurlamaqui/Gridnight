using UnityEngine;

public abstract class PanelUISO : ScriptableObject
{
    private BasePanelUI uiPanel;
    public bool IsOnFocus { get; private set; }

    public virtual void AssignPanel(BasePanelUI panel)
    {
        uiPanel = panel;
    }

    public void Open()
    {
        if (uiPanel == null)
        {
            return;
        }

        uiPanel.SetState(true);
        SetFocus(true);
    }

    public void Close()
    {
        if (uiPanel == null)
        {
            return;
        }

        uiPanel.SetState(false);
        SetFocus(false);
    }

    public void SwitchState()
    {
        if (uiPanel == null)
        {
            return;
        }

        if (uiPanel.IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void SetFocus(bool state)
    {
        if(uiPanel == null)
        {
            return;
        }

        IsOnFocus = state;
    }

    public T GetPanel<T>() where T : BasePanelUI
    {
        return uiPanel as T;
    }
}
