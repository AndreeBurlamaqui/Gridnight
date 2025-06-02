using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{  
    #region SINGLETON

    public static WaveManager Instance { get; private set; }

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

    [SerializeField] private ItemSO[] possibleFoods;

    [Header("SETTINGS")]
    [SerializeField] private int baseTypes = 1;
    [SerializeField] private int baseQuantity = 3;
    [SerializeField] private float difficultyMultiplier = 1.5f;

    public int MaximumPossibleFoods => possibleFoods.Length;

    Dictionary<ItemSO, int> curWaveRequirements = new Dictionary<ItemSO, int>();

    private void Start()
    {
        GenerateWaveRequirements(1);
    }

    public void GenerateWaveRequirements(int waveNumber)
    {
        curWaveRequirements.Clear();
        int typesRequired = Mathf.Min(baseTypes + Mathf.FloorToInt(waveNumber / 2f), MaximumPossibleFoods);

        var randomTypes = Randomizer.CreateRandomOrder(typesRequired, true);
        for (int r = 0; r < randomTypes.Length; r++)
        {
            var foodItem = possibleFoods[randomTypes[r]];
            int quantity = Mathf.RoundToInt(baseQuantity * Mathf.Pow(difficultyMultiplier, waveNumber));

            // Some randomness variation
            quantity += Random.Range(-1, 2);
            quantity = Mathf.Max(1, quantity);

            curWaveRequirements.Add(foodItem, quantity);
        }
    }

    public IEnumerable<(ItemSO item, int amount)> LoopWaveRequirements()
    {
        foreach(var requirement in curWaveRequirements)
        {
            yield return (requirement.Key, requirement.Value);
        }
    }
}
