using System;
using UnityEngine;
using UnityEngine.Events;
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
    public UnityEvent<Vector2Int> OnNavigate;
    public UnityEvent OnInteract;
    public UnityEvent OnInventory;

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
        var inputValue = ctx.ReadValue<Vector2>();
        Vector2 roundedInput = new Vector2(Mathf.Round(inputValue.x), Mathf.Round(inputValue.y));
        moveDirection = holdingMove ? roundedInput : Vector2.zero;
        holdingMove = moveDirection != Vector2.zero; // Sometimes it's skipping phases. So if it's zeroed now, it's not moving
    }

    public void OnInteractInput(CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        OnInteract?.Invoke();
        switch (inputType)
        {
            case MapType.GAMEPLAY:
                if(curInteractOption != null)
                {
                    curInteractOption.Interact();
                }
                break;

            case MapType.UI:
                break;
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

    public void OnInventoryInput(CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        OnInventory?.Invoke();
    }
}
