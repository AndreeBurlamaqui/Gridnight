using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField, TextArea(2, 4)] public string Description { get; private set; }
    [field: SerializeField] public int StartAmount { get; private set; }
    [SerializeField] private ItemSO foodItem;
    [SerializeField] private int foodAmount;

    [field: Header("RUNTIME")]
    [field: SerializeField] public int TotalAmount { get; private set; }
    [field: SerializeField] public Vector2Int SlotGridPosition { get; private set; }

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetData();
        }
    }
#endif

    public void ResetData()
    {
        TotalAmount = StartAmount;
        SlotGridPosition = new Vector2Int(-1, -1);
    }

    public void Add(int amount)
    {
        TotalAmount += amount;
    }

    public (ItemSO item, int amount) GetFood() => (foodItem, foodAmount * TotalAmount);
   
    public void UpdateSlotPosition(int x, int y)
    {
        SlotGridPosition.Set(x, y);
    }
}
