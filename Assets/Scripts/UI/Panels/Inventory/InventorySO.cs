using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/Panels/Inventory")]
public class InventorySO : PanelUISO
{

    public List<ItemSO> items = new();

}
