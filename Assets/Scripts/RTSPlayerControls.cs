using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CommandMode
{
    None,
    Move,
    AttackMove
}

[System.Serializable]
public struct CommandCursors
{
    public Texture2D defaultCursor;
    public Texture2D moveCursor;
    public Texture2D attackCursor;
}

public class RTSPlayerControls : MonoBehaviour
{
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Vector2 mouseScreenPos;
    [SerializeField] private SelectionBox selectionBox;
    public Vector2 MouseScreenPos => mouseScreenPos;
    private Vector3 worldPosition;

    private bool isMouseHeld = false;
    private Vector2 mousetStartPosition; // Position of the mouse when they first press it.

    [SerializeField] private CommandMode selectedCommand = CommandMode.None;
    [SerializeField] private CommandCursors commandCursors;
    


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


        worldPosition = new Vector3(0, 0, 0);
        Ray r = Camera.main.ScreenPointToRay(MouseScreenPos);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            worldPosition = hit.point;
        }
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

    public void OnRightClick(InputAction.CallbackContext context)
    {
        float clickValue = context.ReadValue<float>();

        if (clickValue > 0) // Button pressed
        {
            OnRightClickStarted();
        }
        else // Button released
        {
            OnRightClickEnded();
        }
    }


    private void OnRightClickStarted()
    {
        switch (selectedCommand)
        {
            case CommandMode.None:
                break;
            case CommandMode.Move:
                RTSPlayer.instance.UnitManager.MoveOrder(worldPosition);
                break;
            case CommandMode.AttackMove:
                break;
        }

        SetCommandMode(CommandMode.None);


        
    }
    private void OnRightClickEnded()
    {
        
    }



    /// <summary>
    /// Called whenever the player presses their middle mouse
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

    public void OnClearAction(InputAction.CallbackContext context)
    {
        ClearCommand();
    }

    public void SetCommandMode(CommandMode _mode)
    {
        selectedCommand = _mode;

        Texture2D cursorIcon = commandCursors.defaultCursor;

        switch(_mode)
        {
            case CommandMode.None:
                {
                    cursorIcon = commandCursors.defaultCursor;
                    break;
                }
            case CommandMode.Move:
                {
                    cursorIcon = commandCursors.moveCursor;
                    break;
                }
            case CommandMode.AttackMove:
                {
                    cursorIcon = commandCursors.attackCursor;
                    break;
                }
        }

        Cursor.SetCursor(cursorIcon, default, CursorMode.Auto);
    }

    public void SetCommandMode(int _modeIndex)
    {
        CommandMode mode = (CommandMode)_modeIndex;

        SetCommandMode(mode);
    }

    private void SetCursor(Cursor _cursor)
    {

    }

    public void ClearCommand()
    {
        SetCommandMode(CommandMode.None);
    }

}
