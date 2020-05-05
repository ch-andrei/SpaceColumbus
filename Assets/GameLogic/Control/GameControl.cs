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

namespace Controls
{
    public class GameControl : MonoBehaviour
    {
        #region Configuration

        public bool showGui = true;

        [Header("Selection GUI")] public Color guiColor = new Color(0.8f, 0.8f, 0.95f, 0.25f);
        public Color guiBorderColor = new Color(0.8f, 0.8f, 0.95f);
        public int guiBorderWidth = 2;

        #endregion

        public enum ControlMode : byte
        {
            Default,
            Menu,
            DebugMenu,
            Other
        }

        public ControlMode controlMode;
        public Vector3 mouseOverWorldPosition;

        public GameSession gameSession { get; private set; }

        #region PrivateVariables

        private EventSystem _eventSystem;
        private UiManager _uiManager;

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

        private DebugMenu _debugMenu;

        #endregion DebugUi

        #endregion

        void Start()
        {
            controlMode = ControlMode.Default;

            _debugMenu = this.GetComponent<DebugMenu>();

            _isBoxSelecting = false;

            mouseOverWorldPosition = new Vector3();

            gameSession = (GameSession) GameObject.FindGameObjectWithTag(StaticGameDefs.GameSessionTag)
                .GetComponent(typeof(GameSession));

            _eventSystem = (EventSystem) GameObject.FindGameObjectWithTag(StaticGameDefs.EventSystemTag)
                .GetComponent(typeof(EventSystem));
            _uiManager = (UiManager) GameObject.FindGameObjectWithTag(StaticGameDefs.UiManagerTag)
                .GetComponent(typeof(UiManager));

            // setup key manager
            KeyActiveManager.NewDoubleDetector(GameControlsManager.LeftClickDown.keyPress);
            KeyActiveManager.NewDoubleDetector(GameControlsManager.RightClickDown.keyPress);

            // create GUI style
            _guiStyle = new GUIStyle();
            _guiStyle.alignment = TextAnchor.LowerLeft;
            _guiStyle.normal.textColor = Tools.HexToColor("#153870");
        }

        public void DebugMode()
        {
            controlMode = ControlMode.DebugMenu;
            ResetSelecting();
        }

        void OnMouse0Down()
        {
            switch (controlMode)
            {
                case ControlMode.Default:
                    break;
                case ControlMode.DebugMenu:
                    _debugMenu.OnMouse0();
                    break;
                default:
                    controlMode = ControlMode.Default;
                    break;
            }
        }

        // by default, Control1 is the right mouse click
        void OnMouse1Down()
        {
            switch (controlMode)
            {
                case ControlMode.Default:
                    MoveSelectedAgents();
                    break;
                default:
                    _debugMenu.ResetMode();
                    controlMode = ControlMode.Default;
                    break;
            }
        }

        private void MoveSelectedAgents()
        {
            gameSession.MoveSelectedAgents(mouseOverWorldPosition);
        }

        private void StopSelectedAgents()
        {
            gameSession.StopSelectedAgents();
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
                gameSession.CurrentPlayer.ownership.info
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

            SelectionManager.UpdateSelected(_mousePositionAtSelectionStart, _mousePositionAtSelectionEnd,
                _mouseOverObject, selectionCriteria);
        }

        private void OnDeselectObjects()
        {
            _uiManager.OnEvent(new AgentUiActive(false));

            SelectionManager.DeselectAll();

            _startedBoxSelection = false;
        }

        private void FixedUpdate()
        {
            if (!IsMouseOverUi())
                ProcessSelectionArea();
        }

        void Update()
        {
            KeyActiveManager.Update(); // process key presses

            // trace a ray to cursor location
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                mouseOverWorldPosition = hitInfo.point;
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

            if (controlMode == ControlMode.Default)
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

        void OnGUI()
        {
            if (showGui)
            {
                if (controlMode == ControlMode.Default)
                {
                    if (_isBoxSelecting)
                    {
                        // Create a rect from both mouse positions
                        var rect = OnGuiUtil.GetScreenRect(_mousePositionAtSelectionStart, Input.mousePosition);
                        OnGuiUtil.DrawScreenRect(rect, guiColor);
                        OnGuiUtil.DrawScreenRectBorder(rect, guiBorderWidth, guiBorderColor);
                    }
                }
            }
        }
    }
}