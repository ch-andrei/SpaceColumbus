using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Utilities.Misc;
using Utilities.Events;

using InputControls;
// using Pathfinding;
using Regions;
using Entities;
using Players;
using EntitySelection;

using UI.Utils;
using UI.Menus;
using UnityEngine.Serialization;


public class GameControl : MonoBehaviour
{
    #region Configuration

    public bool showGui = true;

    [Header("Selection GUI")]
    public Color guiColor = new Color(0.8f, 0.8f, 0.95f, 0.25f);
    public Color guiBorderColor = new Color(0.8f, 0.8f, 0.95f);
    public int guiBorderWidth = 2;

    [Header("Debug GUI")]
    public bool allowDebugMenu = true;
    public int guiMenuWidth = 200;
    public int guiMenuHeight = 300;
    public int buttonHeight = 25;
    public int toggleIconSize = 25;

    #endregion

    public enum ControlMode : byte
    {
        Normal,
        SpawnAgent,
        SpawnExplosion,
        SpawnObject,
        Other
    }

    public static string ActionSpawnAgentName = "Spawn Agent";
    public static string ActionSpawnExplosionName = "Spawn Explosion";
    public static string ActionSpawnObjectName = "Spawn Object";

    #region PrivateVariables

    private EventSystem _eventSystem;

    private ControlMode _controlMode;

    private GameSession _gameSession;
    private UiManager _uiManager;

    private Vector3 _mouseOverWorldPosition;

    #region UnitSelection
    private GameObject _mouseOverObject;
    private bool _isBoxSelecting, _startedBoxSelection;
    private Vector2 _mousePositionAtSelectionStart, _mousePositionAtSelectionEnd;
    #endregion UnitSelection

    #region DebugUi
    private static GUIStyle _guiStyle;

    private bool _showDebugGui;
    private int _debugGuiButtonCount;
    private Vector2 _debugGuiScrollPosition = Vector2.zero;
    private Texture2D _debugUiIcon;
    #endregion DebugUi

    #endregion

    void Start()
    {
        _controlMode = ControlMode.Normal;

        _debugUiIcon = Tools.LoadTexture("Assets/GameView/UI/OnGui/Icons/debug.png");

        _isBoxSelecting = false;

        _mouseOverWorldPosition = new Vector3();

        _gameSession = (GameSession)GameObject.FindGameObjectWithTag(StaticGameDefs.GameSessionTag).GetComponent(typeof(GameSession));

        _eventSystem = (EventSystem)GameObject.FindGameObjectWithTag(StaticGameDefs.EventSystemTag).GetComponent(typeof(EventSystem));
        _uiManager = (UiManager)GameObject.FindGameObjectWithTag(StaticGameDefs.UiManagerTag).GetComponent(typeof(UiManager));

        // setup key manager
        KeyActiveManager.NewDoubleDetector(GameControlsManager.LeftClickDown.keyPress);
        KeyActiveManager.NewDoubleDetector(GameControlsManager.RightClickDown.keyPress);

        // create GUI style
        _guiStyle = new GUIStyle();
        _guiStyle.alignment = TextAnchor.LowerLeft;
        _guiStyle.normal.textColor = Tools.HexToColor("#153870");
    }

    public void SpawnAgent()
    {
        _gameSession.SpawnSimpleAgent(_mouseOverWorldPosition);
    }

    public void SpawnExplosive()
    {
        // TODO

        //if (!IsMouseOverUi())
        //    gameSession.SpawnExplosive(mouseOverWorldPosition);
    }

    void OnMouse0Down()
    {
        if (_controlMode == ControlMode.Normal)
        {
            
        }
        else if (_controlMode == ControlMode.SpawnAgent)
        {
            SpawnAgent();
            _controlMode = ControlMode.Normal;
        }
        else if (_controlMode == ControlMode.SpawnAgent)
        {
            SpawnExplosive();
            _controlMode = ControlMode.Normal;
        }
        else
        {
            _controlMode = ControlMode.Normal;
        }
    }

    // by default, Control1 is the right mouse click
    void OnMouse1Down()
    {
        if (_controlMode == ControlMode.Normal)
        {
            MoveSelectedAgents();
        }
        else
        {
            _controlMode = ControlMode.Normal;
        }
    }

    public void MoveSelectedAgents()
    {
        _gameSession.MoveSelectedAgents(_mouseOverWorldPosition);
    }

    public void StopSelectedAgents()
    {
        _gameSession.StopSelectedAgents();
    }

    public bool IsMouseOverUi()
    {
        return _eventSystem.IsPointerOverGameObject();
    }

    void GetSelectionArea()
    {
        // If the left mouse button is pressed, save mouse location and begin selection
        if (!IsMouseOverUi() && KeyActiveManager.IsActive(GameControlsManager.LeftClickDown))
        {
            _startedBoxSelection = true;
            _isBoxSelecting = true;
            _mousePositionAtSelectionStart = Input.mousePosition;
            SelectionManager.Dirty = true;
        }

        if (_isBoxSelecting && KeyActiveManager.IsActive(GameControlsManager.LeftClick))
        {
            _mousePositionAtSelectionEnd = Input.mousePosition;
        }

        // If the left mouse button is released, end selection
        if (KeyActiveManager.IsActive(GameControlsManager.LeftClickUp))
        {
            _isBoxSelecting = false;
        }
    }

    void ResetSelecting()
    {
        _isBoxSelecting = false;

        OnDeselectObjects();
    }

    void ProcessSelectionArea()
    {
        // placeholder selection
        SelectionCriteria selectionCriteria = new SelectionCriteria(
            true, false, true,
            SelectionCriteria.ECondition.Or,
            _gameSession.CurrentPlayer.ownership.info
            );

        // TODO ADD CRITERIA/SORTING of selected objects

        if (!_isBoxSelecting)
        {

            var selectedObjects = SelectionManager.CurrentlySelectedGameObjects;

            try
            {
                if (selectedObjects.Count == 1)
                {
                    var selectedObject = selectedObjects[0];
                    var entity = selectedObject.GetComponent<Entity>();

                    if (entity != null)
                    {
                        _uiManager.OnEvent(new SelectedEntityEvent(entity));

                        if (entity is Agent agent)
                        {
                            _uiManager.OnEvent(new AgentUiActive(true));
                        }
                    }
                }
                else
                {
                    if (selectedObjects.Count == 0)
                    {
                        SelectionManager.UpdateMouseSelection(_mouseOverObject, null);
                    }
                }
            }
            catch (MissingReferenceException e)
            {
                // if object gets destroyed, it may still be referenced here if selection manager doesnt update 'currently selected'
                Debug.Log("Warning: trying to inspect a destroyed object.");
            }

            return;
        }

        if (_startedBoxSelection)
        {
            OnDeselectObjects();
        }

        SelectionManager.UpdateSelected(_mousePositionAtSelectionStart, _mousePositionAtSelectionEnd, _mouseOverObject, selectionCriteria);
    }

    private void OnDeselectObjects()
    {
        _uiManager.OnEvent(new AgentUiActive(false));

        SelectionManager.DeselectAll();

        _startedBoxSelection = false;
    }

    private void FixedUpdate()
    {
        ProcessSelectionArea();
    }

    void Update()
    {
        KeyActiveManager.Update(); // process key presses

        // trace a ray to cursor location
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            _mouseOverWorldPosition = hitInfo.point;
            //Debug.Log(mouseWorldPosition);
            GameObject hitObject = hitInfo.collider.transform.gameObject;
            if (hitObject == null)
            {
                // nothing to do
            }
            else
            {
                _mouseOverObject = hitObject;
            }
        }

        if (_controlMode == ControlMode.Normal)
        {
            GetSelectionArea();
        }
        else
        {
            
        }

        ProcessControls();
    }

    private void ProcessControls()
    {
        if (!IsMouseOverUi())
        {
            if (KeyActiveManager.IsActive(GameControlsManager.LeftClickDown))
                OnMouse0Down();
        
            if (KeyActiveManager.IsActive(GameControlsManager.RightClick))
                OnMouse1Down();
        }

        if (KeyActiveManager.IsActive(GameControlsManager.AgentStopHotkey))
            StopSelectedAgents();
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        OnGuiUtil.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        OnGuiUtil.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        OnGuiUtil.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        OnGuiUtil.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    void OnGuiDebugMenu()
    {
        // DEBUG BUTTONS
        if (_showDebugGui)
        {
            GUI.BeginGroup(new Rect(0, toggleIconSize, guiMenuWidth, guiMenuHeight));

            if (GUI.Button(new Rect(0, 0, guiMenuWidth, buttonHeight), "Spawn Agent"))
            {
                _controlMode = ControlMode.SpawnAgent;
                ResetSelecting();
            }

            if (GUI.Button(new Rect(0, buttonHeight, guiMenuWidth, buttonHeight), "Spawn Explosion"))
            {
                _controlMode = ControlMode.SpawnExplosion;
                ResetSelecting();
            }

            GUI.EndGroup();
        }
        
        // DEBUG ICON
        GUI.Box(new Rect(0, 0, toggleIconSize, toggleIconSize), _debugUiIcon);
        if (GUI.Button(new Rect(0, 0, toggleIconSize, toggleIconSize), new GUIContent("", "Toggle Debug GUI")))
        {
            _showDebugGui = !_showDebugGui;
        }
        GUI.Label(new Rect(10, toggleIconSize, guiMenuWidth, guiMenuHeight), GUI.tooltip);
    }

    void OnGUI()
    {
        if (showGui)
        {
            if (_controlMode == ControlMode.Normal)
            {
                if (_isBoxSelecting)
                {
                    // Create a rect from both mouse positions
                    var rect = OnGuiUtil.GetScreenRect(_mousePositionAtSelectionStart, Input.mousePosition);
                    OnGuiUtil.DrawScreenRect(rect, guiColor);
                    OnGuiUtil.DrawScreenRectBorder(rect, guiBorderWidth, guiBorderColor);
                }
            }

            string controlActionName = "";
            if (_controlMode == ControlMode.SpawnAgent)
            {
                controlActionName = ActionSpawnAgentName;
            }
            else if (_controlMode == ControlMode.SpawnExplosion)
            {
                controlActionName = ActionSpawnExplosionName;
            }
            else if (_controlMode == ControlMode.SpawnObject)
            {
                controlActionName = ActionSpawnObjectName;
            }

            if (controlActionName != "")
                GUI.Label(new Rect(Input.mousePosition.x + 25, Screen.height - Input.mousePosition.y + 25, 200, 25),
                    controlActionName);

            if (allowDebugMenu)
                OnGuiDebugMenu();
        }
    }
}
