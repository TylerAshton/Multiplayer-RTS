using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Input Reader", menuName = "Player/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions
{
    public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>(); // Creates a public Vector2 to hold the input values of the players movement input
    public Vector2 Point => inputActions.Player.Point.ReadValue<Vector2>(); // Creates a public Vector2 to hold the input values of the players pointer

    InputSystem_Actions inputActions; // Reference to the players input action. This holds all the binds that the user will implement

    void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
        }
    }

    public void AutoToggle()
    {
        if (inputActions.Player.enabled)
        {
            inputActions.Disable();
        }
        else
        {
            inputActions.Enable();
        }
    }

    public void ManualToggle(bool _state)
    {
        if (_state)
        {
            inputActions.Enable();
        }
        else
        {
            inputActions.Disable();
        }
    }

    /// <summary>
    /// A public function to enable the players input
    /// </summary>
    public void Enable()
    {
        inputActions.Enable(); // Enables the input for the user
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        
    }

    public void OnAttack2(InputAction.CallbackContext context)
    {
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
    }

    public void OnShop(InputAction.CallbackContext context)
    {
        
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }

    public void OnDebugRevive(InputAction.CallbackContext context)
    {
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
    }
}
