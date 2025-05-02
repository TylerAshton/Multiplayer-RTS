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

[RequireComponent(typeof(CameraSpawner), typeof(CameraMovement))]
public class RTSPlayerControls : MonoBehaviour
{
    [SerializeField] private GameObject rtsCanvasPrefab;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Vector2 mouseScreenPos = Vector3.zero;
    [SerializeField] private SelectionBox selectionBox;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    public Vector2 MouseScreenPos => mouseScreenPos;
    private Vector3 worldPosition;

    private bool isMouseHeld = false;
    private Vector2 mousetStartPosition; // Position of the mouse when they first press it.

    [SerializeField] private CommandMode selectedCommand = CommandMode.None;
    public CommandMode SelectedCommand => selectedCommand;
    [SerializeField] private CommandCursors commandCursors;
    private CameraSpawner cameraSpawner;
    

    /// <summary>
    /// Spawns in the UI canvas and initialises the CameraSpawner and CameraMovement scripts
    /// </summary>
    public void Init()
    {
        GameObject canvas = Instantiate(rtsCanvasPrefab);
        RTSCanvas rtsCanvas = canvas.GetComponent<RTSCanvas>();

        selectionBox = rtsCanvas.selectionBox;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

        if(!TryGetComponent<CameraSpawner>(out cameraSpawner))
        {
            Debug.LogError("CameraSpawner is missing");
        }
        cameraSpawner.Init();

        if(!TryGetComponent<CameraMovement>(out cameraMovement)) // Would rather assure this in require comp but unity is fucked
        {
            Debug.LogError("Camera movement is missing");
        }
        cameraMovement.Init();

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

    /// <summary>
    /// Relays a zoom axis to the CameraMovement script when the player uses the scrollWheel
    /// </summary>
    /// <param name="context"></param>
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

    /// <summary>
    /// Calls the MouseClickStarted and Ended scripts when the player uses left click. Provided they're not clicking the ui
    /// </summary>
    /// <param name="context"></param>
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

    /// <summary>
    /// If the player isn't using the UI, enables the selectionBox
    /// </summary>
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

    /// <summary>
    /// Draws the selection box based on where the player started holding down the mouse and where it is now
    /// </summary>
    private void OnMouseClickHeld()
    {
        selectionBox.DrawSelectionBox(mousetStartPosition, MouseScreenPos);
    }

    /// <summary>
    /// Sends the selectionBox ScreenRect to the unitManager to attempt and AreaSelection,
    /// with said selection updates the cursor based on units selected
    /// </summary>
    private void OnMouseClickEnded()
    {
        if (isUsingUI(mouseScreenPos))
        {
            return;
        }

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

    /// <summary>
    /// Calls the MouseRightClickStarted and Ended scripts when the player uses right click
    /// </summary>
    /// <param name="context"></param>
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

    /// <summary>
    /// If the player isn't using the UI, inacts the order that's tied to the currentCommandMode
    /// </summary>
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

    /// <summary>
    /// Calls the ClearCommand function
    /// </summary>
    /// <param name="context"></param>
    public void OnClearAction(InputAction.CallbackContext context)
    {
        ClearCommand();
    }

    /// <summary>
    /// Sets the command mode, updating the cursorIcon to the associated cursor
    /// </summary>
    /// <param name="_mode"></param>
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

    /// <summary>
    /// Accessable SetCommandMode overlap to be used by UnityButtons
    /// </summary>
    /// <param name="_modeIndex"></param>
    public void SetCommandMode(int _modeIndex)
    {
        CommandMode mode = (CommandMode)_modeIndex;

        SetCommandMode(mode);
    }

    /// <summary>
    /// Sets command mode to None
    /// </summary>
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
        // Scan all GraphicRaycasters in the scene
        foreach (GraphicRaycaster raycaster in FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None)) // TODO This is very performance intensive to find stuff every frame
        {
            if (raycaster.gameObject.name == "RTS SelectionCanvas(Clone)") // TODO: This it yet another reason why this this function is shit
            {
                continue;
            }
            raycaster.Raycast(eventData, results);
            if (results.Count > 0)
                return true;
        }

        return results.Count > 0;
    }
}
