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
    [field: SerializeField, NonSerialized] public int TotalAmount { get; private set; }

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
    }

    public void Add(int amount)
    {
        TotalAmount += amount;
    }

    public (ItemSO food, int amount) GetFood() => (foodItem, foodAmount * TotalAmount);
   
}
