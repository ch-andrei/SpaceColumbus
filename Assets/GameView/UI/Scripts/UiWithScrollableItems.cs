using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Menus
{
    [System.Serializable]
    public class UiWithScrollableItems : MonoBehaviour
    {
        public UiTextField titleTextLeft = new UiTextField();
        public UiTextField titleTextRight = new UiTextField();

        public GameObject contentRoot;

        public virtual void Awake()
        {
            titleTextLeft.Initialize();
            titleTextRight.Initialize();
        }

        void OnValidate()
        {
            Awake();
        }
    }
}
