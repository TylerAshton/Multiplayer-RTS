using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Input Reader", menuName = "Player/Input Reader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
{
    public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>(); // Creates a public Vector2 to hold the input values of the players movement input

    PlayerInputActions inputActions; // Reference to the players input action. This holds all the binds that the user will implement

    public enum InputForPlayer
    {
        Move,
        Shop
    }

    public InputForPlayer moveInput = InputForPlayer.Move;

    void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
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
}
