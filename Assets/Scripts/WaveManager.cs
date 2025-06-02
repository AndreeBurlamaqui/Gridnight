using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private BaseEntity nexusEntity;
    [SerializeField] private Image waveSlider;

    [Header("SETTINGS")]
    [SerializeField] private int baseRequirementTypes = 1;
    [SerializeField] private int baseRequiremntQuantity = 3;
    [SerializeField] private float difficultyMultiplier = 1.5f;
    [SerializeField] private float waveTime = 25;
    [SerializeField] private int basePathLength = 10;
    [SerializeField] private int baseEnemyQuantity = 5;

    [Header("ENVIRONMENT")]
    [SerializeField] private RuleTile pathTile;
    [SerializeField] private SpawnableItem[] spawnableItems;
    [System.Serializable]
    public class SpawnableItem
    {
        public BaseEntity entityPrefab;
        public ItemSO representedItem;
    }

    [Header("CONSTRUCTS")]
    [SerializeField] private BaseEntity constructPrefab;
    [SerializeField] private float constructLifetime = 60;
    private List<Construct> activatedConstructs = new();
    [System.Serializable]
    public class Construct
    {
        public BaseEntity instance;
        public bool IsActive { get; private set; }

        public Construct(BaseEntity _instance)
        {
            instance = _instance;
            SetActive(false);
        }

        public void SetActive(bool newState)
        {
            IsActive = newState;
        
            if(instance.TryGetModule(out VisualModule visual))
            {
                visual.Model.DOFade(IsActive ? 1 : 0.25f, 0.25f);
            }
        }
    }

    [Header("ENEMIES")]
    [SerializeField] private BaseEntity[] enemiesPrefabs;
    [SerializeField] private float enemyJumpSpeed = 0.35f;
    private List<MovingEnemy> spawnedEnemies = new();
    private class MovingEnemy
    {
        public BaseEntity entity;
        public int currentPathIndex;

        public MovingEnemy(BaseEntity entity)
        {
            this.entity = entity;
            currentPathIndex = 0;
        }
    }

    public int MaximumPossibleFoods => possibleFoods.Length;

    Dictionary<ItemSO, (int amountRequired, int amountAchieved)> curWaveRequirements = new();
    private int curWave;
    private List<Vector2Int> wavePath = new();
    WaitForSeconds waitWaveBuild = new WaitForSeconds(0.015f);

    private void Start()
    {
        curWave = 0;
        StartNewWave();
        StartCoroutine(MoveEnemies());
    }

    private void StartNewWave()
    {
        curWave++;
        waveSlider.DOFillAmount(1, waveTime).From(0).OnComplete(OnWaveFinish);
        GenerateWaveRequirements(curWave);

        // Create path
        WorldGrid.Instance.ClearTilemap();
        StartCoroutine(GenerateRandomPath());
        BuildConstructNearPath();

        // Populate with new environment but avoid path
        StartCoroutine(PopulateEnvironment());

        // And start spawning enemies
        int enemyCount = Mathf.RoundToInt(baseEnemyQuantity * Mathf.Pow(difficultyMultiplier, curWave));
        StartCoroutine(SpawnEnemiesOverWave(enemyCount));
    }

    public void GenerateWaveRequirements(int waveNumber)
    {
        curWaveRequirements.Clear();
        int typesRequired = Mathf.Min(baseRequirementTypes + Mathf.FloorToInt(waveNumber / 2f), MaximumPossibleFoods);

        var randomTypes = Randomizer.CreateRandomOrder(typesRequired, true); // NOTE: When type required = 1 it'll always be rock
        for (int r = 0; r < randomTypes.Length; r++)
        {
            var foodItem = possibleFoods[randomTypes[r]];
            int quantity = Mathf.RoundToInt(baseRequiremntQuantity * Mathf.Pow(difficultyMultiplier, waveNumber));

            // Some randomness variation
            quantity += Random.Range(-1, 2);
            quantity = Mathf.Max(1, quantity);

            curWaveRequirements.Add(foodItem, (quantity, 0));
        }
    }

    public IEnumerator GenerateRandomPath()
    {
        // Start will always be on the right side of the screen
        Vector2Int start = new Vector2Int(WorldGrid.Instance.WorldSize.x - 1, Random.Range(0, WorldGrid.Instance.WorldSize.y));
        Vector2Int end = WorldGrid.Instance.WorldToGrid(nexusEntity.transform.position);

        wavePath = new List<Vector2Int> { start };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };

        Vector2Int current = start;
        var gridSize = WorldGrid.Instance.WorldSize;
        int pathLength = Mathf.RoundToInt(basePathLength * Mathf.Pow(difficultyMultiplier, curWave));
        Debug.Log($"Generating a path with {pathLength} steps");
        for (int step = 0; step < pathLength; step++)
        {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();

            if (current.x > 0) possibleMoves.Add(new Vector2Int(current.x - 1, current.y));
            if (current.x < gridSize.x - 1) possibleMoves.Add(new Vector2Int(current.x + 1, current.y));
            if (current.y > 0) possibleMoves.Add(new Vector2Int(current.x, current.y - 1));
            if (current.y < gridSize.y - 1) possibleMoves.Add(new Vector2Int(current.x, current.y + 1));

            if (possibleMoves.Count == 0)
            {
                Debug.LogWarning($"No possible moves from {current}, terminating path early at step {step}");
                break;
            }

            // Sort moves to prioritize getting closer to end
            possibleMoves.Sort((a, b) =>
                Vector2Int.Distance(a, end).CompareTo(Vector2Int.Distance(b, end))
            );

            Vector2Int nextMove;

            if (Random.value < 0.7f)
            {
                nextMove = possibleMoves[0];
            }
            else
            {
                nextMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            }

            Debug.Log($"Step {step + 1}/{pathLength}: Moving to {nextMove}");

            if (visited.Contains(nextMove))
            {
                continue;  // Skip if already visited
            }

            wavePath.Add(nextMove);
            visited.Add(nextMove);
            current = nextMove;

            if (current == end)
            {
                Debug.Log("Reached the end early.");
                break;
            }
        }

        for (int w = 0; w < wavePath.Count; w++)
        {
            WorldGrid.Instance.PaintTile((Vector3Int)wavePath[w], pathTile);
            yield return waitWaveBuild;
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
                if (amountRequired > 0)
                {
                    return true;
                }
                else
                {
                    break; // It's already filled
                }
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
            if( requirement.amountAchieved >= requirement.amountRequired)
            {
                ActivateNextConstruct();
            }
            Debug.Log($"Requirement {item.Title} {requirement.amountAchieved}/{requirement.amountRequired}");
        }
    }

    public void HealNexus(int amount)
    {
        if(!nexusEntity.TryGetModule(out HealthModule health))
        {
            return;
        }

        health.HealBy(amount);
    }

    private void OnWaveFinish()
    {
        StartNewWave();
    }

    private IEnumerator PopulateEnvironment()
    {
        foreach (var requirement in curWaveRequirements)
        {
            ItemSO item = requirement.Key;
            int amountToSpawn = requirement.Value.amountRequired;

            if (!TryGetSpawnableForItem(item, out var spawnable))
            {
                Debug.LogWarning($"No spawnable entity for item {item.name}");
                continue;
            }

            for (int i = 0; i < amountToSpawn; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();

                Instantiate(spawnable.entityPrefab, spawnPos, Quaternion.identity);
                yield return waitWaveBuild;
            }
        }
    }

    private bool TryGetSpawnableForItem(ItemSO item, out SpawnableItem spawnItem)
    {
        List<SpawnableItem> matchingSpawnables = new List<SpawnableItem>();

        foreach (var spawnable in spawnableItems)
        {
            if (spawnable.representedItem == item)
            {
                matchingSpawnables.Add(spawnable);
            }
        }

        if(matchingSpawnables.Count > 0)
        {
            spawnItem = matchingSpawnables.RandomContent();
            return true;
        }

        spawnItem = null;
        return false;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2Int gridSize = WorldGrid.Instance.WorldSize;

        Vector2Int randomGridPos;
        do
        {
            randomGridPos = new Vector2Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
        }
        while (wavePath.Contains(randomGridPos)); // Avoid spawning on the path

        return WorldGrid.Instance.GridToWorld(randomGridPos);
    }

    private void BuildConstructNearPath()
    {
        // Find candidate tiles adjacent to path but not in path
        List<Vector2Int> sideTiles = new List<Vector2Int>();

        foreach (var pathTile in wavePath)
        {
            var adjacentTiles = GetAdjacentTiles(pathTile);

            foreach (var tile in adjacentTiles)
            {
                if (!wavePath.Contains(tile) && !sideTiles.Contains(tile))
                {
                    sideTiles.Add(tile);
                }
            }
        }

        if (sideTiles.Count == 0)
        {
            Debug.LogWarning("No available side tiles for construct activation.");
            return;
        }

        Vector2Int selectedTile = sideTiles[Random.Range(0, sideTiles.Count)];
        Vector3 spawnPos = WorldGrid.Instance.GridToWorld(selectedTile);

        BaseEntity newConstruct = Instantiate(constructPrefab, spawnPos, Quaternion.identity);
        DOVirtual.DelayedCall(constructLifetime, () => OnConstructFadeOut(newConstruct));
        activatedConstructs.Add(new Construct(newConstruct));

        Debug.Log($"Construct activated at {spawnPos}.");
    }

    private void OnConstructFadeOut(BaseEntity constructDead)
    {
        for(int a = activatedConstructs.Count - 1; a >= 0; a--)
        {
            var activeConstruct = activatedConstructs[a];
            if (activeConstruct.instance == constructDead)
            {
                activatedConstructs.RemoveAt(a);
                break;
            }
        }
    }

    private void ActivateNextConstruct()
    {
        for(int a = 0; a < activatedConstructs.Count; a++)
        {
            var activeConstruct = activatedConstructs[a];
            if (!activeConstruct.IsActive)
            {
                activeConstruct.SetActive(true);
                //if(activeConstruct.instance.TryGetModule(out AttackNearbyModule atk))
                //{

                //}
                break;
            }
        }
    }

    private List<Vector2Int> GetAdjacentTiles(Vector2Int tile)
    {
        List<Vector2Int> adjacents = new List<Vector2Int>();

        adjacents.Add(tile + Vector2Int.up);
        adjacents.Add(tile + Vector2Int.down);
        adjacents.Add(tile + Vector2Int.left);
        adjacents.Add(tile + Vector2Int.right);

        return adjacents;
    }

    private IEnumerator SpawnEnemiesOverWave(int totalEnemies)
    {
        float spawnInterval = waveTime / totalEnemies;
        Debug.Log($"Wave {curWave}: Spawning {totalEnemies} over {waveTime} each {spawnInterval} seconds");
        for (int i = 0; i < totalEnemies; i++)
        {
            var spawnPos = wavePath[0];
            var enemyPrefab = enemiesPrefabs.RandomContent();
            var newEnemy = Instantiate(enemyPrefab, WorldGrid.Instance.GridToWorld(spawnPos.x, spawnPos.y), Quaternion.identity);
            spawnedEnemies.Add(new MovingEnemy(newEnemy));
            yield return new WaitForSeconds(spawnInterval); // TODO: Pool it
        }
    }

    private IEnumerator MoveEnemies()
    {
        var waitJump = new WaitForSeconds(enemyJumpSpeed);
        while (true)
        {
            if (spawnedEnemies == null)
            {
                yield return null;
                continue;
            }

            yield return waitJump;
            yield return null; // To  ensure that every spawned enemy will be initiated

            for (int e = 0; e < spawnedEnemies.Count; e++)
            {
                var enemy = spawnedEnemies[e];
                enemy.currentPathIndex += 1;
                if (!enemy.entity.TryGetModule(out MovementModule movement))
                {
                    continue; 
                }

                if (enemy.currentPathIndex >= wavePath.Count)
                {
                    // Reached nexus. Attack and then destroy
                    enemy.entity.Hit(nexusEntity);
                    nexusEntity.Hit(enemy.entity); // the enemy health will be the attack count
                }
                else
                {
                    // Still moving towards end
                    WorldGrid.Instance.RequestMoveGridPosition(enemy.entity, wavePath[enemy.currentPathIndex]);
                }
                Debug.Log($"Enemy {enemy.entity.gameObject.name} moving {enemy.currentPathIndex}/{wavePath.Count}");
            }
        }
    }

    private void DestroyEnemy(int index)
    {
        var enemy = spawnedEnemies[index];
        Destroy(enemy.entity.gameObject);
        spawnedEnemies.RemoveAt(index);
    }
}
