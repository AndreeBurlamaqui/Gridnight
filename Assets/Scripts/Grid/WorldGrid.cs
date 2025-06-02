using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGrid : MonoBehaviour
{
    #region SINGLETON

    public static WorldGrid Instance { get; private set; }

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

    [field: SerializeField] public Vector2Int WorldSize { get; private set; }
    [SerializeField] private int cellSize;
    [SerializeField] private Tilemap tilemap;
    
    private BaseGrid<BaseEntity> entitiesGrid;

    public bool IsInitiated { get; private set; }
    public Vector2Int Origin => new Vector2Int((int)transform.position.x, (int)transform.position.y) + (WorldSize / 2);

    private void Start()
    {
        entitiesGrid = new BaseGrid<BaseEntity>(WorldSize.x, WorldSize.y, cellSize);
        IsInitiated = true;
        gameObject.GetOrAddComponent<Grid>().cellSize = cellSize * Vector3.one;
    }

    public void Spawn(BaseEntity entity, Vector3 worldPos)
    {
        var gridPos = WorldToGrid(worldPos);
        SetEntityPosition(entity, gridPos);

        if(entity.TryGetModule(out HealthModule health))
        {
            health.OnDeath.AddListener(OnEntityDeath);
        }
    }

    private void OnEntityDeath(HealthModule healthEntity)
    {
        // Unsubscribe then remove from grid
        Debug.Log($"Entity {healthEntity.gameObject.name} is dead, removing from grid");
        healthEntity.OnDeath.RemoveListener(OnEntityDeath);
        if(entitiesGrid.TryGetPosition(healthEntity.Entity, out int oldX, out int oldY))
        {
            entitiesGrid.SetValue(oldX, oldY, null);
        }
        else
        {
            Debug.LogWarning($"Entity {healthEntity.gameObject.name} was not in the grid. Can't remove");
        }
    }

    public void RequestMoveDirection(BaseEntity entity, Vector2 direction)
    {
        if (!IsInitiated || direction == Vector2.zero)
        {
            // Missconfig
            return;
        }

        if (!entitiesGrid.TryGetPosition(entity, out int x, out int y))
        {
            // Entity is not in grid
            return;
        }

        var curGrid = new Vector2(x, y);
        RequestMoveGridPosition(entity, direction + curGrid);
    }

    public void RequestMoveGridPosition(BaseEntity entity, Vector2 nextGrid)
    {
        if (!IsInitiated)
        {
            // Missconfig
            return;
        }

        if (!entity.TryGetModule(out MovementModule movement) || !movement.CanJump)
        {
            // Entity can't move
            return;
        }

        int x = (int)nextGrid.x;
        int y = (int)nextGrid.y;
        if (entitiesGrid.TryGetValue(x, y, out var otherEntity))
        {
            if (otherEntity != null)
            {
                // Notify entity that it's touching something
                entity.Hit(otherEntity);
            }
            else
            {
                // Should be removed from grid
                entitiesGrid.SetValue(x, y, null);
            }
        }
     
        // Free space, can move
        //Debug.Log($"Entity {entity.gameObject.name} is on {curGrid} requesting to move towards {direction} -> {nextGrid}");
        SetEntityPosition(entity, nextGrid);
    }

    private void SetEntityPosition(BaseEntity entity, Vector2 targetGrid)
    {
        var nextGrid = entitiesGrid.SetValue((int)targetGrid.x, (int)targetGrid.y, entity);
        var worldPos = GridToWorldCentered(nextGrid);
        if (entity.TryGetModule(out MovementModule movement))
        {
            if (movement.CanJump)
            {
                movement.FreeMove(worldPos);

            }
        }
        else
        {
            // This will be usually for stationary objects that should still be positioned correctly in the grid
            entity.transform.position = worldPos;
        }
    }

    public Vector2 GridToWorld(Vector2 pos) => GridToWorld((int)pos.x, (int)pos.y);
    public Vector2 GridToWorld(int x, int y)
    {
        Vector3 halfSize = new Vector3(WorldSize.x * cellSize, WorldSize.y * cellSize) * 0.5f;
        Vector3 localPos = new Vector3(x * cellSize, y * cellSize);
        return transform.position - halfSize + localPos + (0.5f * cellSize * Vector3.one);
    }

    /// <summary>
    /// World position but centered to the cell size
    /// </summary>
    public Vector2 GridToWorldCentered(Vector2 nextGrid) => GridToWorld(nextGrid) + (0.5f * cellSize * Vector2.one);

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 halfSize = new Vector3(WorldSize.x * cellSize, WorldSize.y * cellSize) * 0.5f;

        // Calculate local position relative to bottom-left corner
        Vector3 localPos = worldPos - (transform.position - halfSize);

        // Get grid indices by dividing by cell size
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);

        return new(x, y);
    }

    public bool TryGetValidPositionAround(Vector3 worldPos, out Vector3 validPos)
    {
        var gridPos = WorldToGrid(worldPos);
        if (!entitiesGrid.IsPositionFilled(gridPos.x, gridPos.y))
        {
            // We can use this one
            validPos = worldPos;
            return true;
        }

        int directionsEnumLength = TinyUtils.GetEnumLength<TinyUtils.EightDirections>();
        var randomOrder = Randomizer.CreateRandomOrder(directionsEnumLength, true);
        for (int d = 0; d < randomOrder.Length; d++)
        {
            var directionToCheck = (TinyUtils.EightDirections)randomOrder[d];
            var checkPos = gridPos + directionToCheck.ToVector2();
            if (entitiesGrid.IsPositionFilled((int)checkPos.x, (int)checkPos.y))
            {
                continue;
            }

            validPos = checkPos;
            return true;
        }

        validPos = Vector3.zero;
        return false;
    }

    public void PaintTile(Vector3Int position, RuleTile tile)
    {
        var worldPos = GridToWorld(position.x, position.y);
        tilemap.SetTile(tilemap.WorldToCell(worldPos), tile);
        tilemap.RefreshAllTiles();
    }

    public void ClearTilemap()
    {
        tilemap.ClearAllTiles();
        tilemap.RefreshAllTiles();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < WorldSize.x; x++)
        {
            for (int y = 0; y < WorldSize.y; y++)
            {
                var gizmosPosition = GridToWorld(x, y) + Vector2.one * (cellSize / 2f);
                var gizmosSize = Vector3.one * cellSize;

                Gizmos.DrawWireCube(gizmosPosition, gizmosSize);
                if (IsInitiated && entitiesGrid.IsPositionFilled(x, y))
                {
                    Gizmos.DrawWireCube(gizmosPosition, gizmosSize * 0.5f);
                }
            }
        }
    }
}
