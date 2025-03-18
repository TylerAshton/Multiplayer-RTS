using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSPlayerControls : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Vector2 mouseScreenPos;
    [SerializeField] private SelectionBox selectionBox;
    public Vector2 MouseScreenPos => mouseScreenPos;

    private bool isMouseHeld = false;
    private Vector2 mousetStartPosition; // Position of the mouse when they first press it.


    private void Awake()
    {
        if (selectionBox == null)
        {
            Debug.LogError("Selection box wasn't selected");
        }

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        if (isMouseHeld) { OnMouseClickHeld(); }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        int axis = (int)context.ReadValue<Vector2>().y;
        cameraMovement.ApplyZoom(axis);
    }

    /// <summary>
    /// This is called all the time to aquire screen position and update the mouseScreenPos variable
    /// </summary>
    /// <param name="context"></param>
    public void OnPoint(InputAction.CallbackContext context)
    {
        mouseScreenPos = context.ReadValue<Vector2>();
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        float clickValue = context.ReadValue<float>();

        if (clickValue > 0) // Button pressed
        {
            OnMouseClickStarted();
        }
        else // Button released
        {
            OnMouseClickEnded();
        }
    }

    private void OnMouseClickStarted()
    {
        isMouseHeld = true;
        mousetStartPosition = mouseScreenPos;
        selectionBox.EnableBox();
    }

    private void OnMouseClickHeld()
    {
        selectionBox.DrawSelectionBox(mousetStartPosition, MouseScreenPos);
    }

    private void OnMouseClickEnded()
    {
        isMouseHeld = false;
        RTSPlayer.instance.UnitManager.AreaSelection(selectionBox.GetScreenRect());
        selectionBox.DisableBox();
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
        cameraMovement.StartPanning(mouseScreenPos);
    }

    private void OnMiddleClickEnded()
    {
        cameraMovement.StopPanning();
    }

}
