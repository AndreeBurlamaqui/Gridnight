using System;
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

    [Header("RUNTIME")]
    [SerializeField] private MapType inputType;
    public Vector2 moveDirection;
    public Vector2 navigateDirection;
    public InteractionOption curInteractOption;
    public bool holdingMove;

    // EVENTS
    public event Action<Vector2Int> OnNavigate;

    public void Initiate()
    {
        // Reset all runtime values
        ChangeType(MapType.GAMEPLAY);
        moveDirection = Vector2.zero;
        navigateDirection = Vector2.zero;
        curInteractOption = null;
        holdingMove = false;
    }

    public void ChangeType(MapType newType)
    {
        inputType = newType;
        Debug.Log("Changing input type to " + newType);
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
        holdingMove = moveDirection != Vector2.zero; // Sometimes it's skipping phases. So if it's zeroed now, it's not moving
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

    public void OnNavigateInput(CallbackContext ctx)
    {
        if(inputType != MapType.UI)
        {
            return;
        }

        if (!ctx.performed)
        {
            return;
        }

        navigateDirection = ctx.ReadValue<Vector2>();
        OnNavigate?.Invoke(new Vector2Int((int)navigateDirection.x, (int)navigateDirection.y));
    }
}
