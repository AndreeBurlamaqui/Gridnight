using UnityEngine;

public abstract class PanelUISO : ScriptableObject
{
    private BasePanelUI uiPanel;

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
    }

    public void Close()
    {
        if (uiPanel == null)
        {
            return;
        }

        uiPanel.SetState(false);
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

    public bool TryGetPanel<T>(out T typePanel) where T : BasePanelUI
    {
        typePanel = default(T);
        if(uiPanel is T targetType)
        {
            typePanel = targetType;
            return true;
        }

        return false;
    }
}
