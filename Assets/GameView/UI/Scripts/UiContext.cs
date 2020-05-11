using System.Collections.Generic;
using UnityEngine;

namespace UI.Components
{
    public enum UiContextType : byte
    {
        Default,
        Menu,
        Other
    }

    [System.Serializable]
    public class UiContext : UiModule
    {
        public UiContextType contextType;

        private List<Canvas> _canvases;

        public int SortingOrder { get; private set; }

        public new void Awake()
        {
            base.Awake();

            this._canvases = new List<Canvas>();
        }

        public void Start()
        {
            foreach (var module in _submodules)
            {
                Canvas canvas = module.GetComponent<Canvas>();
                if (canvas == null) return;

                this._canvases.Add(canvas);
            }
        }

        public void SetOrder(int order)
        {
            SortingOrder = order;

            foreach (var canvas in _canvases)
                canvas.sortingOrder = order;
        }
    }
}
