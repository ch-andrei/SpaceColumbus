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
        
        public bool allowDebug = true;

        [Header("Debug GUI")]
        public int guiMenuWidth = 200;
        public int guiMenuHeight = 300;
        public int buttonHeight = 25;
        public int toggleIconSize = 25;

        private Texture2D _debugUiIcon;

        public enum DebugMode : byte
        {
            SpawnAgent,
            SpawnExplosion,
            SpawnObject,
            Other
        }

        private DebugMode _mode;

        private bool _showDebugGui;

        private GameControl _gameControl;

        public void Start()
        {
            _debugUiIcon = Tools.LoadTexture("Assets/GameView/UI/OnGui/Icons/debug.png");

            _mode = DebugMode.Other;
            _gameControl = this.GetComponent<GameControl>();
        }

        public void OnMouse0()
        {
            switch (_mode)
            {
                case DebugMode.SpawnAgent:
                    _gameControl.gameSession.SpawnSimpleAgent(_gameControl.mouseOverWorldPosition);
                    break;
                case DebugMode.SpawnExplosion:
                    break;
                default:
                    break;
            }
        }

        public void ResetMode()
        {
            _mode = DebugMode.Other;
        }

        private void OnGuiDebugMenu()
        {
            // DEBUG BUTTONS
            if (_showDebugGui)
            {
                GUI.BeginGroup(new Rect(0, toggleIconSize, guiMenuWidth, guiMenuHeight));

                if (GUI.Button(new Rect(0, 0, guiMenuWidth, buttonHeight), ActionSpawnAgentName))
                {
                    _mode = DebugMode.SpawnAgent;
                    _gameControl.DebugMode();
                }

                if (GUI.Button(new Rect(0, buttonHeight, guiMenuWidth, buttonHeight), ActionSpawnExplosionName))
                {
                    _mode = DebugMode.SpawnExplosion;
                    _gameControl.DebugMode();
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
                case DebugMode.SpawnAgent:
                    controlActionName = ActionSpawnAgentName;
                    break;
                case DebugMode.SpawnExplosion:
                    controlActionName = ActionSpawnExplosionName;
                    break;
                case DebugMode.SpawnObject:
                    controlActionName = ActionSpawnObjectName;
                    break;
            }

            if (controlActionName != "")
                GUI.Label(new Rect(Input.mousePosition.x + 25, Screen.height - Input.mousePosition.y + 25, 200, 25),
                    controlActionName);
        }
    }
}
