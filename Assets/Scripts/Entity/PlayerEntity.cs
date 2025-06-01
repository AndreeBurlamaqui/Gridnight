using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerEntity : BaseEntity
{
    public const string PLAYER_TAG = "Player";

    [SerializeField] private GameObject interactionArrow;

    private Vector2 moveDirection;
    private InteractionOption curInteractOption;

    protected override void Initiate()
    {
        base.Initiate();
        SetPossibleInteraction(null);
        Debug.Log("Initiating player");
    }

    public void OnMoveInput(CallbackContext ctx)
    {
        bool holdingMove = false;
        switch (ctx.phase)
        {
            case UnityEngine.InputSystem.InputActionPhase.Disabled:
                holdingMove = false;
                break;
            case UnityEngine.InputSystem.InputActionPhase.Waiting:
                break;
            case UnityEngine.InputSystem.InputActionPhase.Started:
                holdingMove = true;
                break;
            case UnityEngine.InputSystem.InputActionPhase.Performed:
                holdingMove = true;
                break;
            case UnityEngine.InputSystem.InputActionPhase.Canceled:
                holdingMove = false;
                break;
        }

        //Debug.Log("On move input " + ctx.phase);
        moveDirection = holdingMove ? ctx.ReadValue<Vector2>() : Vector2.zero;
    }

    private void Update()
    {
        if (moveDirection != Vector2.zero)
        {
            WorldGrid.Instance.RequestMove(this, moveDirection);
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
            if(interactable == curInteractOption)
            {
                SetPossibleInteraction(null);
            }
        }
    }

    private void SetPossibleInteraction(InteractionOption interactable)
    {
        curInteractOption = interactable;
        interactionArrow.SetActive(curInteractOption != null);
    }

    public void OnInteractInput(CallbackContext ctx)
    {
        if(curInteractOption == null)
        {
            return;
        }

        if (ctx.performed)
        {
            curInteractOption.Interact();
        }
    }
}