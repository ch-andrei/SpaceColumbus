using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using UnityEngine;

namespace UI.Menus
{
    [System.Serializable]
    public class UiContext
    {
        public string Name;

        public List<Canvas> canvases = new List<Canvas>();

        public UiContext()
        {
            canvases = new List<Canvas>();
        }

        public void SetActive(bool active)
        {
            foreach (var canvas in canvases)
                canvas.gameObject.SetActive(active);
        }
        
        public void SetOrder(int order)
        {
            foreach (var canvas in canvases)
                canvas.sortingOrder = order;
        }
    }

    [System.Serializable]
    public class UiContextManager
    {
        public UiContext DefaultContext;
        public UiContext Menu1Context;
        public UiContext Menu2Context;
        public UiContext Menu3Context;

        public int Count => _contextStack.Count;

        private Stack<UiContext> _contextStack;
        
        public UiContextManager()
        {
            Initialize();
        }

        public void Initialize()
        {
            _contextStack = new Stack<UiContext>();
        }
        
        public void PushContext(UiContext context)
        {
            _contextStack.Push(context);
            context.SetOrder(Count);
        }

        public UiContext PopContext()
        {
            var context = _contextStack.Pop();

            context.SetActive(false);
            context.SetOrder(0);

            return _contextStack.Pop();
        }
    }
}