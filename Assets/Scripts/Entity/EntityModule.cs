using UnityEngine;

public class EntityModule : MonoBehaviour
{
    public BaseEntity Entity { get; private set; }

    public virtual void Initiate(BaseEntity controller)
    {
        Debug.Log($"Entity {gameObject.name} initiating module {GetType()}");
        Entity = controller;
    }

    public virtual void Terminate()
    {

    }
}
