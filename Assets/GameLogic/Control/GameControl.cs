using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

using Entities;
using EntitySelection;

using GameLogic;
using Players;

using Animation;

using UI.Utils;
using UI;

using InputControls;


namespace Controls
{
    public class GameControl : MonoBehaviour
    {
        private enum EControlType
        {
            Default,
            Menu,
            DebugMenu,
            Other
        }

        #region Configuration

        public bool showGui = true;

        [Header("Selection GUI")]
        public Color guiColor = new Color(0.8f, 0.8f, 0.95f, 0.25f);
        public Color guiBorderColor = new Color(0.8f, 0.8f, 0.95f);
        public int guiBorderWidth = 2;

        #endregion

        public Vector3 mouseOverWorldPosition;

        public GameManager gameManager { get; private set; }
        public GameSession gameSession { get; private set; }

        #region PrivateVariables

        private EControlType _controlType;

        private UiManager _uiManager;
        private EventSystem _eventSystem;

        private DebugMenu _debugMenu;

        #region UnitSelection
        private GameObject _mouseOverObject;
        private bool _isBoxSelecting, _startedBoxSelection;
        private Vector2 _mousePositionAtSelectionStart, _mousePositionAtSelectionEnd;
        #endregion UnitSelection

        #endregion

        private void Awake()
        {
            gameManager = (GameManager) GameObject.FindGameObjectWithTag(StaticGameDefs.GameManagerTag)
                .GetComponent(typeof(GameManager));
            gameSession = gameManager.gameSession;

            _uiManager = (UiManager) GameObject.FindGameObjectWithTag(StaticGameDefs.UiManagerTag)
                .GetComponent(typeof(UiManager));

            _eventSystem = (EventSystem) GameObject.FindGameObjectWithTag(StaticGameDefs.EventSystemTag)
                .GetComponent(typeof(EventSystem));
        }

        private void Start()
        {
            _debugMenu = this.GetComponent<DebugMenu>();

            _isBoxSelecting = false;

            mouseOverWorldPosition = new Vector3();

            // setup key manager
            KeyActiveManager.NewDoubleDetector(GameControlsManager.LeftClickDown.keyPress);
            KeyActiveManager.NewDoubleDetector(GameControlsManager.RightClickDown.keyPress);

            DefaultMode();
        }

        private void Update()
        {
            RayToCursorPosition();

            ProcessControls();

            if (!IsMouseOverUi())
                ProcessSelectionArea();
        }

        private void RayToCursorPosition()
        {
            // trace a ray to cursor location
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                mouseOverWorldPosition = hitInfo.point;
                var hitObject = hitInfo.collider.transform.gameObject;
                if (hitObject is null)
                {
                    // nothing to do
                }
                else
                {
                    _mouseOverObject = hitObject;
                }
            }
        }

        private void ProcessControls()
        {
            KeyActiveManager.Update(); // process key presses

            if (KeyActiveManager.IsActive(GameControlsManager.LeftClickDown))
                OnMouse0Down();

            if (KeyActiveManager.IsActive(GameControlsManager.LeftClick))
                OnMouse0Hold();

            if (KeyActiveManager.IsActive(GameControlsManager.LeftClickUp))
                OnMouse0Up();

            if (KeyActiveManager.IsActive(GameControlsManager.RightClickDown))
                OnMouse1Down();

            if (KeyActiveManager.IsActive(GameControlsManager.AgentStopHotkey))
                OrderStopSelectedAgents();
        }

        private void ProcessSelectionArea()
        {
            // If selecting with mouse pointer only (no box selection)
            if (!_isBoxSelecting)
            {
                var selectedObjects = SelectionManager.GetSelectedObjects();

                try
                {
                    if (selectedObjects.Count == 1)
                    {
                        var selectedObject = selectedObjects[0];
                        var entity = EntityManager.GetEntityInParent(selectedObject);

                        if (entity != null)
                        {
                            _uiManager.OnSelectEntity(entity);
                        }
                    }
                    else if (selectedObjects.Count == 0)
                        SelectionManager.UpdateMouseSelection(_mouseOverObject, null);
                }
                catch (MissingReferenceException e)
                {
                    // if object gets destroyed, it may still be referenced here if selection manager doesnt update 'currently selected'
                    Debug.Log("Warning: trying to inspect a destroyed object.");
                    SelectionManager.CheckMissingSelected(); // TODO: optionally remove this
                }

                return;
            }

            if (_startedBoxSelection)
            {
                _startedBoxSelection = false;
                OnDeselectObjects();
            }

            // placeholder selection
            var selectionCriteria = new SelectionCriteria(
                isAgent: true,
                isBuilding: false,
                isControllable: true,
                SelectionCriteria.ECondition.Or,
                gameSession.CurrentPlayer.ownership.info
            );

            // TODO ADD CRITERIA/SORTING of selected objects

            SelectionManager.UpdateSelected(
                _mousePositionAtSelectionStart, _mousePositionAtSelectionEnd,
                _mouseOverObject, selectionCriteria);
        }

        public void SetDebugMenu()
        {
            _controlType = EControlType.DebugMenu;
            ResetSelection();
            ResetUi();
        }

        public void SetMenu(int menu)
        {
            _controlType = EControlType.Menu;
            _uiManager.OpenMenu(menu);
        }

        private void OnMouse0Down()
        {
            switch (_controlType)
            {
                case EControlType.Default:
                    if (!IsMouseOverUi())
                        SelectionStart();
                    break;
                case EControlType.DebugMenu:
                    _debugMenu.OnMouse0();
                    break;
                default:
                    break;
            }
        }

        private void OnMouse0Hold()
        {
            if (_controlType == EControlType.Default)
                _mousePositionAtSelectionEnd = Input.mousePosition;
        }

        private void OnMouse0Up()
        {
            if (_controlType == EControlType.Default)
                _isBoxSelecting = false;
        }

        // by default, Control1 is the right mouse click
        private void OnMouse1Down()
        {
            switch (_controlType)
            {
                case EControlType.Default:
                    OrderMoveSelectedAgents();
                    break;
                case EControlType.Menu:
                    int depth = _uiManager.CloseNewestMenu();
                    if (depth <= 1)
                        DefaultMode();
                    break;
                case EControlType.DebugMenu:
                    _debugMenu.ResetMode();
                    DefaultMode();
                    break;
            }
        }

        private void SelectionStart()
        {
            if (KeyActiveManager.IsActive(GameControlsManager.LeftClickDown))
            {
                _startedBoxSelection = true;
                _isBoxSelecting = true;
                _mousePositionAtSelectionStart = Input.mousePosition;
                SelectionManager.Dirty = true;
            }
        }

        public void DefaultMode()
        {
            _controlType = EControlType.Default;
            ResetSelection();
        }

        private void OrderMoveSelectedAgents()
        {
            gameSession.MoveSelected(mouseOverWorldPosition);
        }

        private void OrderStopSelectedAgents()
        {
            gameSession.StopSelectedAgents();
        }

        public bool IsMouseOverUi()
        {
            return _eventSystem.IsPointerOverGameObject();
        }

        private void ResetSelection()
        {
            _isBoxSelecting = false;
            _startedBoxSelection = false;
            OnDeselectObjects();
        }

        private void ResetUi()
        {
            _uiManager.Reset();
        }

        private void OnDeselectObjects()
        {
            SelectionManager.DeselectAll();
            _uiManager.OnDeselect();
        }

        private void OnGUI()
        {
            if (showGui)
            {
                if (_controlType == EControlType.Default)
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
