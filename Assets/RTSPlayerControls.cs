using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSPlayerControls : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Vector2 screenPosition;
    public Vector2 ScreenPosition => screenPosition;
    

    public void OnScroll(InputAction.CallbackContext context)
    {
        int axis = (int)context.ReadValue<Vector2>().y;
        cameraMovement.ApplyZoom(axis);
    }

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
    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        float clickValue = context.ReadValue<float>();

        if (clickValue > 0) // Button pressed
        {
            OnMiddleClickStarted();
        }
        else // Button released
        {
            OnMiddleClickEnded();
        }
    }
    private void OnMiddleClickStarted()
    {
        cameraMovement.StartPanning(screenPosition);
    }

    private void OnMiddleClickEnded()
    {
        cameraMovement.StopPanning();
    }

}
