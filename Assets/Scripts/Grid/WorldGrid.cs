using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.UI.Image;

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

    [SerializeField] private Vector2Int worldSize;
    [SerializeField] private int cellSize;
    private BaseGrid<BaseEntity> entitiesGrid;
    private Tilemap tilemap;

    public bool IsInitiated { get; private set; }
    public Vector2Int Origin => new Vector2Int((int)transform.position.x, (int)transform.position.y) + (worldSize / 2);

    private void Start()
    {
        entitiesGrid = new BaseGrid<BaseEntity>(worldSize.x, worldSize.y, cellSize);
        IsInitiated = true;
    }

    public void Spawn(BaseEntity entity, int x, int y)
    {
        if (entity.TryGetModule(out MovementModule movement))
        {
            SetEntityPosition(movement, entitiesGrid.SetValue(x, y, entity));
        }
    }

    public void RequestMove(BaseEntity entity, Vector2 direction)
    {
        if(!IsInitiated || direction == Vector2.zero)
        {
            return;
        }

        if (!entity.TryGetModule(out MovementModule movement) || !movement.CanJump)
        {
            return;
        }

        if (entitiesGrid.TryGetPosition(entity, out int x, out int y))
        {
            var curGrid = new Vector2(x, y);
            var nextGrid = direction.normalized + curGrid;
            Debug.Log($"Entity {entity.gameObject.name} is on {curGrid} requesting to move towards {direction} -> {nextGrid}");
            SetEntityPosition(movement, entitiesGrid.SetValue((int)nextGrid.x, (int)nextGrid.y, entity));
        }
    }

    private void SetEntityPosition(MovementModule entityMov, Vector3 nextGrid)
    {
        entityMov.Move(GridToWorld(nextGrid) + (0.5f * cellSize * Vector2.one));
    }

    public Vector2 GridToWorld(Vector2 pos) => GridToWorld((int)pos.x, (int)pos.y);
    public Vector2 GridToWorld(int x, int y)
    {
        Vector3 halfSize = new Vector3(worldSize.x * cellSize, worldSize.y * cellSize) * 0.5f;
        Vector3 localPos = new Vector3(x * cellSize, y * cellSize);
        return transform.position - halfSize + localPos + (0.5f * cellSize * Vector3.one);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < worldSize.x; x++)
        {
            for (int y = 0; y < worldSize.y; y++)
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
