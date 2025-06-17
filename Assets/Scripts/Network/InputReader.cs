using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Input Reader", menuName = "Player/Input Reader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions
{
    public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>();

    PlayerInputActions inputActions;

    void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
    }

    public void Enable()
    {
        inputActions.Enable();
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
