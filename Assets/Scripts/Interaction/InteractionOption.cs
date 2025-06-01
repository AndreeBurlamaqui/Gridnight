using UnityEngine;
using UnityEngine.Events;

public class InteractionOption : MonoBehaviour
{
    public const string INTERACTION_TAG = "Interaction";

    public UnityEvent OnInteract;

    public void Interact()
    {
        OnInteract?.Invoke();
    }
}
