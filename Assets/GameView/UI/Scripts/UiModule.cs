using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Components
{
    [System.Serializable]
    public class UiModule : MonoBehaviour
    {
        // Name of the UI module
        public string label;

        protected List<UiModule> _submodules;

        public void Awake()
        {
            _submodules = GetComponentsInChildren<UiModule>().ToList();
        }

        public void SetActive(bool active)
        {
            this.gameObject.SetActive(active);
        }
    }
}
