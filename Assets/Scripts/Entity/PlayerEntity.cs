using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerEntity : BaseEntity
{
    public const string PLAYER_TAG = "Player";

    [SerializeField] private InputReader playerInput;
    [SerializeField] private GameObject interactionArrow;

    protected override void Initiate()
    {
        base.Initiate();
        SetPossibleInteraction(null);
        playerInput.Initiate();
        Debug.Log("Initiating player");
    }

    private void Update()
    {
        if (playerInput.holdingMove)
        {
            WorldGrid.Instance.RequestMoveDirection(this, playerInput.moveDirection);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(InteractionOption.INTERACTION_TAG) &&
            collision.TryGetComponent(out InteractionOption interactable))
        {
            SetPossibleInteraction(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(InteractionOption.INTERACTION_TAG) &&
            collision.TryGetComponent(out InteractionOption interactable))
        {
            if(interactable == playerInput.curInteractOption)
            {
                SetPossibleInteraction(null);
            }
        }
    }

    private void SetPossibleInteraction(InteractionOption interactable)
    {
        interactionArrow.SetActive(interactable != null);
        playerInput.curInteractOption = interactable;
    }
}