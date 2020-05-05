//
//Filename: KeyboardCameraControl.cs
//

using System;
using System.Collections;
using UnityEngine;

using Regions;
using InputControls;

namespace Controls
{

[AddComponentMenu("Camera/CameraControl")]
public class CameraControl : MonoBehaviour
{
    public float globalSensitivity = 10f; // global camera speed sensitivity
    public float cameraSpeedModifierMultiplier = 2.5f; // global camera speed sensitivity multipler when a special modifer key is held down
    
    #region MouseControlConfiguration

    // camera scrolling sensitivity
    [Header("Scrolling")]
    public float scrollingSensitivityModifier = 10f;

    // edge scrolling
    [Header("Edge scrolling")]
    public bool allowEdgeScrolling = false;
    public int edgeScrollDetectBorderThickness = 15;

    // mouse control camera translation
    [Header("Mouse Scrolling")]
    public bool allowMouseTranslation = true;
    public float mouseTranslationSensitivityModifier = 0.75f; // mouse translation movement speed modifier

    // mouse rotation control
    [Header("Mouse Rotation")]
    public bool allowMouseRotation = true;
    public float mouseRotationSensitivityModifier = 50f; // mouse rotation movement speed modifier

    // zoom with FOV 
    [Header("Camera zoom")]
    public bool allowCameraZoom = true;
    public float cameraZoomSensitivityModifier = 2f; // mouse zoom speed modifier
    public float cameraFovMin = 30f;
    public float cameraFovMax = 120f;

    #endregion

    #region CameraControlConfiguration

    [Header("Camera movement inertia")]
    public bool allowCameraInertia = true;
    [Range(0.01f, 0.99f)]
    public float inertiaDecay = 0.95f;

    // camera restriction
    [Header("Camera restriction")]

    public float cameraVerticalAngleMin = 10f;
    public float cameraVerticalAngleMax = 80f;

    public float viewCenterOffset = 200f; // camera view center point offset; calculated as this far in front of camera
    public float viewCenterOnPlayerOffset = 75f; // how far from player position the camera will be set when focusing on player
    public float viewCenterOnPlayerLimiterInertia = 0.5f; // how 

    // speed limiter must be adjusted given maxCameraToGroundDistance; shorter max dist requires higher limiter
    public float limiterInertia = 0.1f;

    public float cameraLimitDistance = 500f; // how far camera can move away from the player
    public float minCameraToGroundDistance = 2f; // how close to ground the camera can go before limiter will start resisting
    public float maxCameraToGroundDistance = 200f; // how high camera can go before limiter will start resisting

    public float cameraTooHighSpeedLimiter = 1.5f; // lower means less resistance
    public float cameraTooLowSpeedLimiter = 5f; // this one needs to be resistive otherwise camera will dip into objects

    #endregion

    #region PrivateVariables

    private bool _cameraMoving = false;
    private bool _cameraRotating = false;
    private bool _cameraZooming = false;

    private bool _mouseOverGame = false;

    private Vector3 _mousePositionAtRightClickDown;
    private Vector3 _mousePositionAtMiddleClickDown;

    // inertia
    private Vector3 _inertiaPositionDelta;
    private Vector3 _inertiaRotationDelta;
    private float _inertiaFovDelta;

    private Vector3 _restrictionCenterPoint, _viewCenterPoint;

    private GameControl _gameControl;
    private GameSession _gameSession;
    private Region _region;

    #endregion

    void Start()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

        transform.position = GetCameraViewPointPosition();

        _gameControl = GameObject.FindGameObjectWithTag(StaticGameDefs.GameControlTag).GetComponent<GameControl>();
        _gameSession = GameObject.FindGameObjectWithTag(StaticGameDefs.GameSessionTag).GetComponent<GameSession>();
        _region = _gameSession.GetRegion();

        _restrictionCenterPoint = new Vector3(0, 0, 0); // GameControl.gameSession.humanPlayer.getPos();
        _viewCenterPoint = new Vector3(0, 0, 0);

        _mousePositionAtRightClickDown = Input.mousePosition;
        _mousePositionAtMiddleClickDown = Input.mousePosition;

        _inertiaPositionDelta = Vector3.zero;
        _inertiaRotationDelta = Vector3.zero;
        _inertiaFovDelta = 0;
    }

    void Update()
    {
        _region = _gameSession.GetRegion(); // can do this via events instead

        _cameraMoving = false;
        _cameraRotating = false;
        _cameraZooming = false;

        Vector3 cameraPos = this.transform.position,
            cameraDir = this.transform.forward;

        cameraPos.y = 0;
        cameraDir.y = 0;

        _viewCenterPoint = cameraPos + cameraDir * viewCenterOffset;

        CheckInputConfiguration();

        float modifier = KeyActiveManager.IsActive(GameControlsManager.CameraSpeedModifier) ? cameraSpeedModifierMultiplier : 1f;

        Vector3 positionDelta = ProcessCameraMovement() * modifier;
        Vector3 rotationDelta = ProcessCameraRotation() * modifier;
        float fovDelta = ProcessCameraZoom() * modifier;

        ProcessCameraDeltas(positionDelta, rotationDelta, fovDelta);

        RestrictCamera();
    }

    private void CheckInputConfiguration()
    {
        _mouseOverGame = false;
        // mouse cursor position check
        if (Input.mousePosition.x >= 0 &&
            Input.mousePosition.y >= 0 &&
            Input.mousePosition.x <= Screen.width &&
            Input.mousePosition.y <= Screen.height)
        {
            _mouseOverGame = true;
        }

        _mouseOverGame &= !_gameControl.IsMouseOverUi();

        // on right click
        if (KeyActiveManager.IsActive(GameControlsManager.RightClickDown))
        {
            _mousePositionAtRightClickDown = Input.mousePosition;
        }

        // on middle click
        if (KeyActiveManager.IsActive(GameControlsManager.MiddleClickDown))
        {
            _mousePositionAtMiddleClickDown = Input.mousePosition;
        }
    }

    // keyboard and edge scrolling
    private Vector3 ProcessCameraMovement()
    {
        Vector3 positionDelta = Vector3.zero;

        Vector3 mouseDelta = Input.mousePosition - _mousePositionAtMiddleClickDown;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        if (KeyActiveManager.IsActive(GameControlsManager.CameraForward) ||
            (allowEdgeScrolling && Input.mousePosition.y >= Screen.height - edgeScrollDetectBorderThickness))
        {
            positionDelta += forward;
        }
        if (KeyActiveManager.IsActive(GameControlsManager.CameraBack) ||
            (allowEdgeScrolling && Input.mousePosition.y <= edgeScrollDetectBorderThickness))
        {
            positionDelta -= forward;
        }
        if (KeyActiveManager.IsActive(GameControlsManager.CameraLeft) ||
            (allowEdgeScrolling && Input.mousePosition.x <= edgeScrollDetectBorderThickness))
        {
            positionDelta -= right;
        }
        if (KeyActiveManager.IsActive(GameControlsManager.CameraRight) ||
            (allowEdgeScrolling && Input.mousePosition.x >= Screen.width - edgeScrollDetectBorderThickness))
        {
            positionDelta += right;
        }
        if (KeyActiveManager.IsActive(GameControlsManager.CameraDown))
        {
            positionDelta += Vector3.down;
        }
        if (KeyActiveManager.IsActive(GameControlsManager.CameraUp))
        {
            positionDelta += Vector3.up;
        }

        // scrolling with mouse
        if (allowMouseTranslation && KeyActiveManager.IsActive(GameControlsManager.MiddleClick))
        {
            if (_mouseOverGame)
            {
                Vector3 mouseTranslation = Vector3.zero;
                mouseTranslation += right * mouseDelta.x / Screen.width;
                mouseTranslation += forward * mouseDelta.y / Screen.height;

                positionDelta += mouseTranslation * mouseTranslationSensitivityModifier;
            }
        }

        positionDelta *= scrollingSensitivityModifier * globalSensitivity * Time.deltaTime;

        if (Vector3.zero != positionDelta)
            _cameraMoving = true;

        return positionDelta;
    }

    private Vector3 ProcessCameraRotation()
    {
        Vector3 rotation = Vector3.zero;

        if (allowMouseRotation && KeyActiveManager.IsActive(GameControlsManager.RightClick)) // right mouse
        {
            if (_mouseOverGame)
            {

                Vector3 mouseDelta = Input.mousePosition - _mousePositionAtRightClickDown;

                rotation += Vector3.up * mouseDelta.x / Screen.width; // horizontal
                rotation += Vector3.left * mouseDelta.y / Screen.height; // vertical

                rotation *= mouseRotationSensitivityModifier * globalSensitivity * Time.deltaTime;

                if (Vector3.zero != rotation)
                    _cameraRotating = true;
            }
        }

        return rotation;
    }

    private float ProcessCameraZoom()
    {
        float fovDelta = 0;

        if (allowCameraZoom)
        {
            if (_mouseOverGame)
            {
                // camera zoom via FOV change
                fovDelta = Input.mouseScrollDelta.y * cameraZoomSensitivityModifier;

                if (fovDelta != 0)
                    _cameraZooming = true;
            }
        }

        return fovDelta;
    }

    private void ProcessCameraDeltas(Vector3 positionDelta, Vector3 rotationDelta, float fovDelta)
    {
        if (allowCameraInertia)
        {
            _inertiaPositionDelta = _inertiaPositionDelta * inertiaDecay + positionDelta * (1f - inertiaDecay);
            _inertiaRotationDelta = _inertiaRotationDelta * inertiaDecay + rotationDelta * (1f - inertiaDecay);
            _inertiaFovDelta = _inertiaFovDelta * inertiaDecay + fovDelta * (1f - inertiaDecay);
        }

        // apply position delta
        transform.Translate(_inertiaPositionDelta, Space.World);

        // apply rotation delta
        transform.localEulerAngles += _inertiaRotationDelta;

        // apply zoom delta
        Camera.main.fieldOfView -= _inertiaFovDelta;
    }

    private void RestrictCamera()
    {
        // check if camera is out of bounds 
        Vector3 posRelative = transform.position - _restrictionCenterPoint;
        if (posRelative.x > cameraLimitDistance)
        {
            transform.position -= new Vector3(posRelative.x - cameraLimitDistance, 0, 0);
        }
        else if (posRelative.x < -cameraLimitDistance)
        {
            transform.position -= new Vector3(posRelative.x + cameraLimitDistance, 0, 0);
        }
        if (posRelative.z > cameraLimitDistance)
        {
            transform.position -= new Vector3(0, 0, posRelative.z - cameraLimitDistance);
        }
        else if (posRelative.z < -cameraLimitDistance)
        {
            transform.position -= new Vector3(0, 0, posRelative.z + cameraLimitDistance);
        }

        // adjust camera height based on terrain
        float waterLevel = 0; // GameControl.gameSession.mapGenerator.getRegion().getWaterLevelElevation();
        float offsetAboveWater = transform.position.y - (waterLevel) - minCameraToGroundDistance;
        if (offsetAboveWater < 0)
        { // camera too low based on water elevation
            transform.position -= new Vector3(0, offsetAboveWater, 0) * limiterInertia * cameraTooLowSpeedLimiter;
        }
        try
        {
            Vector3 tileBelow = _region.GetTileAt(transform.position).Pos;

            float offsetAboveFloor = transform.position.y - (tileBelow.y) - minCameraToGroundDistance;
            float offsetBelowCeiling = tileBelow.y + maxCameraToGroundDistance - (transform.position.y);

            if (offsetAboveFloor < 0)
            { // camera too low based on tile height
                transform.position -= new Vector3(0, offsetAboveFloor, 0) * limiterInertia * cameraTooLowSpeedLimiter;
            }
            else if (offsetBelowCeiling < 0)
            { // camera too high 
                transform.position += new Vector3(0, offsetBelowCeiling, 0) * limiterInertia * cameraTooHighSpeedLimiter;
            }
        }
        catch (NullReferenceException e)
        {
            // do nothing
        }

        // restrict rotation
        Vector3 rotation = transform.localEulerAngles;
        rotation.x = Mathf.Clamp(rotation.x, cameraVerticalAngleMin, cameraVerticalAngleMax);
        rotation.z = 0;
        transform.localEulerAngles = rotation;

        // restrict fov
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, cameraFovMin, cameraFovMax);
    }

    // approximate location where the camera is looking
    public Vector3 GetCameraViewPointPosition()
    {
        return /*GameControl.gameSession.humanPlayer.getPos()*/ new Vector3(0, 0, 0) - transform.forward * viewCenterOnPlayerOffset;
    }
}
}