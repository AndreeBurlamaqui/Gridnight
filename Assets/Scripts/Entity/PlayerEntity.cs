using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerEntity : BaseEntity
{

    Vector2 moveDirection;

    protected override void Initiate()
    {
        base.Initiate();
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
}