using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSPlayerControls : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    private Vector2 screenPosition;
    public Vector2 ScreenPosition => screenPosition;
    

    /// <summary>
    /// This is called all the time to aquire screen position and update the screenPosition variable
    /// </summary>
    /// <param name="context"></param>
    public void OnPoint(InputAction.CallbackContext context)
    {
        screenPosition = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called whenever the player presses their mouse
    /// </summary>
    /// <param name="context"></param>
    public void Click(InputAction.CallbackContext context)
    {
        float clickValue = context.ReadValue<float>();

        if (clickValue > 0) // Button pressed
        {
            OnClickStarted();
        }
        else // Button released
        {
            OnClickEnded();
        }
    }
    private void OnClickStarted()
    {
        cameraMovement.StartPanning(screenPosition);
    }

    private void OnClickEnded()
    {
        cameraMovement.StopPanning();
    }

}
