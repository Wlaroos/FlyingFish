using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputs : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public InputSystem_Actions InputActions { get; private set; }
    
    public static Vector2 MovementValue { get; private set; }
    public static event Action OnInteractPressed;

    private void Awake()
    {
        InputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        InputActions.Player.Enable();
        InputActions.Player.SetCallbacks(this);
    }

    private void OnDisable()
    {
        InputActions.Player.Disable();
        InputActions.Player.RemoveCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnInteractPressed?.Invoke();
        }
    }
}