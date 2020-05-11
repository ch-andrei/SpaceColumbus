using System;
using UnityEngine;

using Utilities.Misc;

namespace Controls
{
    public class DebugMenu : MonoBehaviour
    {
        private const string ToggleDebugButtonName = "Toggle Debug GUI";
        private const string ActionSpawnAgentName = "Spawn Agent";
        private const string ActionSpawnExplosionName = "Spawn Explosion";
        private const string ActionSpawnObjectName = "Spawn Object";
        private const string Menu1ToggleName = "Open Menu 1";
        private const string Menu2ToggleName = "Open Menu 2";
        private const string Menu3ToggleName = "Open Menu 3";
        
        public bool allowDebug = true;

        [Header("Debug GUI")]
        public int guiMenuWidth = 200;
        public int guiMenuHeight = 300;
        public int buttonHeight = 25;
        public int toggleIconSize = 25;

        public enum EDebugMode : byte
        {
            SpawnAgent,
            SpawnExplosion,
            SpawnObject,
            Other
        }

        private EDebugMode _mode;

        private bool _showDebugGui;

        private Texture2D _debugUiIcon;

        private GameControl _gameControl;

        public void Start()
        {
            _debugUiIcon = Tools.LoadTexture("Assets/GameView/UI/OnGui/Icons/debug.png");

            _mode = EDebugMode.Other;
            _gameControl = this.GetComponent<GameControl>();
        }

        public void OnMouse0()
        {
            switch (_mode)
            {
                case EDebugMode.SpawnAgent:
                    _gameControl.gameSession.SpawnSimpleAgent(_gameControl.mouseOverWorldPosition);
                    break;
                case EDebugMode.SpawnExplosion:
                    break;
                default:
                    break;
            }
        }

        public void ResetMode()
        {
            _mode = EDebugMode.Other;
        }

        private void OnGuiDebugMenu()
        {
            // DEBUG BUTTONS
            if (_showDebugGui)
            {
                GUI.BeginGroup(new Rect(0, toggleIconSize, guiMenuWidth, guiMenuHeight));

                if (GUI.Button(new Rect(0, 0 * buttonHeight, guiMenuWidth, buttonHeight), ActionSpawnAgentName))
                {
                    _mode = EDebugMode.SpawnAgent;
                    _gameControl.SetDebugMenu();
                }

                if (GUI.Button(new Rect(0, 1 * buttonHeight, guiMenuWidth, buttonHeight), ActionSpawnExplosionName))
                {
                    _mode = EDebugMode.SpawnExplosion;
                    _gameControl.SetDebugMenu();
                }
                
                if (GUI.Button(new Rect(0, 2 * buttonHeight, guiMenuWidth, buttonHeight), Menu1ToggleName))
                {
                    _gameControl.SetMenu(1);
                }
                
                if (GUI.Button(new Rect(0, 3 * buttonHeight, guiMenuWidth, buttonHeight), Menu2ToggleName))
                {
                    _gameControl.SetMenu(2);
                }
                
                if (GUI.Button(new Rect(0, 4 * buttonHeight, guiMenuWidth, buttonHeight), Menu3ToggleName))
                {
                    _gameControl.SetMenu(3);
                }

                GUI.EndGroup();
            }

            // DEBUG ICON
            GUI.Box(new Rect(0, 0, toggleIconSize, toggleIconSize), _debugUiIcon);
            if (GUI.Button(new Rect(0, 0, toggleIconSize, toggleIconSize), new GUIContent("", ToggleDebugButtonName)))
            {
                _showDebugGui = !_showDebugGui;
            }

            GUI.Label(new Rect(toggleIconSize + 5, 5, guiMenuWidth, guiMenuHeight), GUI.tooltip);
        }
        
        private void OnGUI()
        {
            if (!allowDebug) return;

            OnGuiDebugMenu();

            string controlActionName = "";
            switch (_mode)
            {
                case EDebugMode.SpawnAgent:
                    controlActionName = ActionSpawnAgentName;
                    break;
                case EDebugMode.SpawnExplosion:
                    controlActionName = ActionSpawnExplosionName;
                    break;
                case EDebugMode.SpawnObject:
                    controlActionName = ActionSpawnObjectName;
                    break;
                default:
                    break;
            }

            if (controlActionName != "")
                GUI.Label(new Rect(Input.mousePosition.x + 25, Screen.height - Input.mousePosition.y + 25, 200, 25),
                    controlActionName);
        }
    }
}
