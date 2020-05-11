using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

using UI.Components;

namespace UI.Fields
{
    [System.Serializable]
    public class UiModuleWithScrollableItems : UiModule
    {
        public UiTextField titleTextLeft;
        public UiTextField titleTextRight;

        public GameObject contentRoot;
    }
}
