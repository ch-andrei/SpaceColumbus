using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities.Events;
using Common;
using System;
using UnityEngine.Serialization;

namespace InputControls
{
    public enum KeyPressType : byte
    {
        Down,
        Up,
        Hold
    }

    public struct KeyPress : IEquatable<KeyPress>, IEqualityComparer<KeyPress>
    {
        public KeyCode Code; // key code
        public KeyPressType Type; // type of press

        public KeyPress(KeyCode code, KeyPressType type)
        {
            Code = code;
            Type = type;
        }

        public bool Equals(KeyPress other)
        {
            return GetHashCode(this) == GetHashCode(other);
        }

        public bool Equals(KeyPress x, KeyPress y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(KeyPress x)
        {
            // id bitwise format: 
            // bit 0-8: code, this does not exceed 512, hence 2^9
            // bit 9-10: type
            return (int)x.Code + (int)x.Type << 10;
        }
    }

    public struct MultiKeyPress
    {
        public int Size;
        public KeyPress[] Keys;

        public MultiKeyPress(int size)
        {
            this.Size = size;
            this.Keys = new KeyPress[size];
        }

        public MultiKeyPress(List<KeyPress> keys)
        {
            this.Size = keys.Count;
            this.Keys = keys.ToArray();
        }
    }

    [System.Serializable]
    public struct ControlInfo
    {
        public KeyPress keyPress;

        public bool isDoubleKey;

        public KeyCode Code
        {
            get => keyPress.Code;
            set => keyPress.Code = value;
        }
        public KeyPressType Type
        {
            get => keyPress.Type;
            set => keyPress.Type = value;
        }

        public ControlInfo(KeyCode code, KeyPressType type, bool isDoubleKey)
        {
            this.keyPress = new KeyPress();
            this.isDoubleKey = isDoubleKey;
            this.Code = code;
            this.Type = type;
        }
    }

    [System.Serializable]
    // Handles left modifiers keys (Alt, Ctrl, Shift)
    public class KeyModifier
    {
        public bool leftAlt;
        public bool leftControl;
        public bool leftShift;

        public bool IsActive()
        {
            return (!leftAlt ^ Input.GetKey(KeyCode.LeftAlt)) &&
                (!leftControl ^ Input.GetKey(KeyCode.LeftControl)) &&
                (!leftShift ^ Input.GetKey(KeyCode.LeftShift));
        }
    }

    [System.Serializable]
    public static class KeyActiveManager
    {
        private struct KeyActiveState
        {
            public bool Down;
            public bool Up;
            public bool Hold;
        }

        private static Dictionary<KeyCode, KeyActiveState> _activeKeys = new Dictionary<KeyCode, KeyActiveState>();

        private static Dictionary<KeyPress, DoubleKeyPressDetector> _doubleKeyPressDetectors =
            new Dictionary<KeyPress, DoubleKeyPressDetector>();

        public static bool IsActive(KeyCode keyCode, KeyPressType onKey = KeyPressType.Down)
        {
            KeyActiveState activeState;

            if (_activeKeys.ContainsKey(keyCode))
            {
                activeState = _activeKeys[keyCode];
            }
            else
            {
                activeState.Down = Input.GetKeyDown(keyCode);
                activeState.Up = Input.GetKeyUp(keyCode);
                activeState.Hold = Input.GetKey(keyCode);
                _activeKeys.Add(keyCode, activeState);
            }

            if (onKey == KeyPressType.Down)
                return activeState.Down;
            else if (onKey == KeyPressType.Up)
                return activeState.Up;
            else if (onKey == KeyPressType.Hold)
                return activeState.Hold;
            else
                return false;
        }

        public static bool IsActive(KeyPress keyPress)
        {
            return IsActive(keyPress.Code, keyPress.Type);
        }

        public static bool IsActive(ControlInfo control)
        {
            if (control.isDoubleKey)
            {
                return IsActiveDouble(control.keyPress);
            }
            else
            {
                return IsActive(control.Code, control.Type);
            }
        }

        public static bool IsActive(ControlInfo[] controls)
        {
            bool active = true;
            foreach (var control in controls)
                active &= IsActive(control);
            return active;
        }

        public static DoubleKeyPressDetector NewDoubleDetector(KeyPress key)
        {
            var detector = new DoubleKeyPressDetector(key);

            // save to existing detectors
            _doubleKeyPressDetectors[key] = detector;

            return detector;
        }

        private static bool IsActiveDouble(KeyPress key)
        {
            if (_doubleKeyPressDetectors.ContainsKey(key))
                return _doubleKeyPressDetectors[key].isActive;
            else
            {
                var detector = NewDoubleDetector(key);
                return detector.isActive;
            }
        }

        public static void Update()
        {
            _activeKeys = new Dictionary<KeyCode, KeyActiveState>();

            foreach (var detector in _doubleKeyPressDetectors.Values)
            {
                detector.Update();
            }
        }
    }

    [System.Serializable]
    public class DoubleKeyPressDetector : IUpdateable
    {
        private KeyPress _key;
        private float _doubleKeyTime;
        private float _timeSinceLastPress;

        [FormerlySerializedAs("IsActive")] public bool isActive;

        public DoubleKeyPressDetector(KeyPress key, float doubleKeyTime = 0.2f)
        {
            this._doubleKeyTime = doubleKeyTime;
            this._timeSinceLastPress = doubleKeyTime;
            this._key = key;

            if (key.Type == KeyPressType.Hold)
                Debug.Log("Warning: Initialized a DoubleKeyPressDetector with key type Hold.");
        }

        public void Update()
        {
            isActive = false;
            _timeSinceLastPress += Time.deltaTime;

            bool timeout = _timeSinceLastPress > _doubleKeyTime;

            if (KeyActiveManager.IsActive(this._key))
            {
                _timeSinceLastPress = 0f;

                if (!timeout)
                {
                    isActive = true;
                }
            }
        }
    }
}