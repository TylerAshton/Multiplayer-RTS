using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private GameObject rtsCanvasPrefab;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Vector2 mouseScreenPos;
    [SerializeField] private SelectionBox selectionBox;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    public Vector2 MouseScreenPos => mouseScreenPos;
    private Vector3 worldPosition;

    private bool isMouseHeld = false;
    private Vector2 mousetStartPosition; // Position of the mouse when they first press it.

    [SerializeField] private CommandMode selectedCommand = CommandMode.None;
    public CommandMode SelectedCommand => selectedCommand;
    [SerializeField] private CommandCursors commandCursors;
    


    private void Awake()
    {
        GameObject canvas = Instantiate(rtsCanvasPrefab);
        RTSCanvas rtsCanvas = canvas.GetComponent<RTSCanvas>();

        selectionBox = rtsCanvas.selectionBox;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

        if(!TryGetComponent<CameraMovement>(out cameraMovement))
        {
            Debug.LogError("Camera movement is missing");
        }

        if (selectionBox == null)
        {
            Debug.LogError("Selection box wasn't selected");
            return;
        }
        if (graphicRaycaster == null)
        {
            Debug.LogError("Graphics reaycaster wasn't selected");
            return;
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
        if (isUsingUI(mouseScreenPos))
        {
            return;
        }

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

        // If a unit was selected switch to move commands if idle
        if (selectedCommand == CommandMode.None)
        {
            if (RTSPlayer.instance.UnitManager.SelectedUnits.Count > 0)
            {
                SetCommandMode(CommandMode.Move);
            }
        }
        else if (selectedCommand != CommandMode.None)
        {
            if (RTSPlayer.instance.UnitManager.SelectedUnits.Count == 0)
            {
                SetCommandMode(CommandMode.None);
            }
        }
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
        if (isUsingUI(mouseScreenPos))
        {
            return;
        }

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

    /// <summary>
    /// Returns a true value if the screenPosition overlaps a space on the canvas of the graphics raycaster
    /// </summary>
    /// <param name="_screenPosition"></param>
    /// <returns></returns>
    private bool isUsingUI(Vector2 _screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = _screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);
        return results.Count > 0;
    }
}
