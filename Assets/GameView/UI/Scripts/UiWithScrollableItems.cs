using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Menus
{
    [System.Serializable]
    public class UiWithScrollableItems : MonoBehaviour
    {
        [FormerlySerializedAs("TitleTextLeft")] public UiTextField titleTextLeft = new UiTextField();
        [FormerlySerializedAs("TitleTextRight")] public UiTextField titleTextRight = new UiTextField();

        [FormerlySerializedAs("ContentRoot")] public GameObject contentRoot;

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
