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

    Dictionary<ItemSO, (int amountRequired, int amountAchieved) > curWaveRequirements = new();

    private void Start()
    {
        GenerateWaveRequirements(1);
    }

    public void GenerateWaveRequirements(int waveNumber)
    {
        curWaveRequirements.Clear();
        int typesRequired = Mathf.Min(baseTypes + Mathf.FloorToInt(waveNumber / 2f), MaximumPossibleFoods);

        var randomTypes = Randomizer.CreateRandomOrder(typesRequired, true); // NOTE: When type required = 1 it'll always be rock
        for (int r = 0; r < randomTypes.Length; r++)
        {
            var foodItem = possibleFoods[randomTypes[r]];
            int quantity = Mathf.RoundToInt(baseQuantity * Mathf.Pow(difficultyMultiplier, waveNumber));

            // Some randomness variation
            quantity += Random.Range(-1, 2);
            quantity = Mathf.Max(1, quantity);

            curWaveRequirements.Add(foodItem, (quantity, 0));
        }
    }

    public IEnumerable<(ItemSO item, int amountRequired, int amountAchieved)> LoopWaveRequirements()
    {
        foreach(var requirement in curWaveRequirements)
        {
            Debug.Log($"Looping wave requirement: {requirement.Key} {requirement.Value.amountAchieved}/{requirement.Value.amountRequired}");
            yield return (requirement.Key, requirement.Value.amountRequired, requirement.Value.amountAchieved);
        }
    }

    public bool IsRequirement(ItemSO item, out int amountRequired)
    {
        foreach(var requirement in LoopWaveRequirements())
        {
            if(requirement.item == item)
            {
                amountRequired = requirement.amountRequired - requirement.amountAchieved;
                return true;
            }
        }

        amountRequired = 0;
        return false;
    }

    public void AddRequirementAchieveAmount(ItemSO item, int amount)
    {
        if(curWaveRequirements.TryGetValue(item, out var requirement))
        {
            requirement.amountAchieved += amount;
            curWaveRequirements[item] = requirement;
            Debug.Log($"Requirement {item.Title} {requirement.amountAchieved}/{requirement.amountRequired}");
        }
    }
}
