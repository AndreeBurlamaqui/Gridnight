using UnityEngine;

public class DropModule : EntityModule
{
    [SerializeField] private GameObject[] options;

    public void Drop()
    {
        var randomDrop = options.RandomContent();
        if(randomDrop == null)
        {
            return;
        }

        // Spawn in a valid position around
        if(WorldGrid.Instance.TryGetValidPositionAround(transform.position, out var validPos))
        {
            Instantiate(randomDrop, validPos, Quaternion.identity);
        }
    }
}
