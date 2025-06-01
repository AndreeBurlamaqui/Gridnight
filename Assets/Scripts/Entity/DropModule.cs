using UnityEngine;

public class DropModule : EntityModule
{
    [SerializeField] private DropTable[] tables;

    public void Drop()
    {
        var randomDrop = tables.RandomContent().GetRandomOption();

        // Spawn in a valid position around
        if(WorldGrid.Instance.TryGetValidPositionAround(transform.position, out var validPos))
        {
            Instantiate(randomDrop, validPos, Quaternion.identity);
        }
    }
}

public struct DropTable
{
    [SerializeField] private GameObject[] options;

    public GameObject GetRandomOption() => options.RandomContent();
}