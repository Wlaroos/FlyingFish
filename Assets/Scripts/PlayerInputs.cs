using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputs : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public InputSystem_Actions InputActions { get; private set; }
    
    public static Vector2 MovementValue { get; private set; }
    public static event Action OnInteractPressed;
    public static event Action OnRestartPressed;
    public static event Action OnEscapePressed;
    public static event Action OnTackleBoxPressed;
    private bool _canFish = true;
    public bool CanFish => _canFish;

    public static PlayerInputs Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

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

    public void OnRestart(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnRestartPressed?.Invoke();
        }

        EnableInputs();
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnEscapePressed?.Invoke();
        }
    }

    public void OnTackleBox(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnTackleBoxPressed?.Invoke();
        }
    }

    public void DisableInputs()
    {
        InputActions.Player.Move.Disable();
        InputActions.Player.TackleBox.Disable();
        _canFish = false;
    }

    public void EnableInputs()
    {
        InputActions.Player.Move.Enable();
        InputActions.Player.TackleBox.Enable();
        _canFish = true;
    }
}