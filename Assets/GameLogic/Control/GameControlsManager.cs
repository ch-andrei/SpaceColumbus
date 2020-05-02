using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using InputControls;

public class GameControlsManager : MonoBehaviour
{
    #region MouseClicks
    public static ControlInfo LeftClick = new ControlInfo(KeyCode.Mouse0, KeyPressType.Hold, false);
    public static ControlInfo LeftClickDown = new ControlInfo(KeyCode.Mouse0, KeyPressType.Down, false);
    public static ControlInfo LeftClickDownDouble = new ControlInfo(KeyCode.Mouse0, KeyPressType.Down, true);
    public static ControlInfo LeftClickUp = new ControlInfo(KeyCode.Mouse0, KeyPressType.Up, false);

    public static ControlInfo RightClick = new ControlInfo(KeyCode.Mouse1, KeyPressType.Hold, false);
    public static ControlInfo RightClickDown = new ControlInfo(KeyCode.Mouse1, KeyPressType.Down, false);
    public static ControlInfo RightClickDownDouble = new ControlInfo(KeyCode.Mouse1, KeyPressType.Down, true);
    public static ControlInfo RightClickUp = new ControlInfo(KeyCode.Mouse1, KeyPressType.Up, false);

    public static ControlInfo MiddleClick = new ControlInfo(KeyCode.Mouse2, KeyPressType.Hold, false);
    public static ControlInfo MiddleClickDown = new ControlInfo(KeyCode.Mouse2, KeyPressType.Down, false);
    #endregion MouseClicks

    #region CameraMovement
    public static ControlInfo CameraForward = new ControlInfo(KeyCode.W, KeyPressType.Hold, false);
    public static ControlInfo CameraBack = new ControlInfo(KeyCode.S, KeyPressType.Hold, false);
    public static ControlInfo CameraLeft = new ControlInfo(KeyCode.A, KeyPressType.Hold, false);
    public static ControlInfo CameraRight = new ControlInfo(KeyCode.D, KeyPressType.Hold, false);
    public static ControlInfo CameraDown = new ControlInfo(KeyCode.Q, KeyPressType.Hold, false);
    public static ControlInfo CameraUp = new ControlInfo(KeyCode.E, KeyPressType.Hold, false);

    public static ControlInfo CameraSpeedModifier = new ControlInfo(KeyCode.LeftShift, KeyPressType.Hold, false);
    #endregion CameraMovement

    public static ControlInfo AgentStopHotkey = new ControlInfo(KeyCode.Space, KeyPressType.Down, false);
}
