using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject
{
    public enum MapType
    {
        GAMEPLAY,
        UI
    }

    [SerializeField] private MapType inputType;

    [Header("RUNTIME")]
    public Vector2 moveDirection;
    public InteractionOption curInteractOption;
    public bool holdingMove;

    public void ChangeType(MapType newType)
    {
        inputType = newType;
    }

    public void SwitchInputType() => ChangeType(inputType == MapType.GAMEPLAY ? MapType.UI : MapType.GAMEPLAY);

    public void OnMoveInput(CallbackContext ctx)
    {
        if(inputType != MapType.GAMEPLAY)
        {
            return;
        }

        holdingMove = false;
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

    public void OnInteractInput(CallbackContext ctx)
    {
        if (inputType != MapType.GAMEPLAY)
        {
            return;
        }

        if (curInteractOption == null)
        {
            return;
        }

        if (ctx.performed)
        {
            curInteractOption.Interact();
        }
    }
}
