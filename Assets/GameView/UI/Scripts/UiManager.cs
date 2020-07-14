using System;
using System.Collections.Generic;
using UnityEngine;

using Entities;

using UI.Menus;
using UI.Fields;
using UI.Components;

namespace UI
{
    /*
     * Class for managing all UI related actions.
     */
    public class UiManager : MonoBehaviour
    {
        #region Contexts and Modules

        public UiContext DefaultContext;
        public UiContext Menu1Context;
        public UiContext Menu2Context;
        public UiContext Menu3Context;

        //public UiComponent EntityUi;
        public UiModule MenuVitalsLog;
        private AgentInspectorUiModule _agentInspectorUi;

        #endregion Contexts and Modules

        private UiContextManager _contextManager;

        private void Awake()
        {
            _agentInspectorUi = MenuVitalsLog.GetComponentInChildren<AgentInspectorUiModule>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            _contextManager = new UiContextManager();
            _contextManager.PushContext(DefaultContext);

            OnSelectAgent(true); // enable, to make sure that it can be disabled
            OnDeselect(); // disable
        }

        public void OpenMenu(int menu)
        {
            switch (menu)
            {
                case 1:
                    _contextManager.PushContext(Menu1Context);
                    break;
                case 2:
                    _contextManager.PushContext(Menu2Context);
                    break;
                case 3:
                    _contextManager.PushContext(Menu3Context);
                    break;
                default:
                    break;
            }
        }

        public int CloseNewestMenu()
        {
            _contextManager.PopContext();
            return _contextManager.Depth;
        }

        public void Reset()
        {
            _contextManager.PopAll();
        }

        public void OnSelectEntity(Entity entity)
        {
            // Debug.Log("OnSelectEntity");

            if (entity.EntityType == EntityType.Agent)
            {
                OnSelectAgent(true);
                this._agentInspectorUi.SetObservedEntity(entity);
            }
            else
            {
                OnDeselect();
                Debug.Log("Selected something that isn't an agent.");
            }
        }

        public void OnSelectAgent(bool selectState) => this._agentInspectorUi.SetActive(selectState);
        public void OnDeselect() => OnSelectAgent(false);
    }

    /*
     * Class for keeping track of Context switches.
     */
    [System.Serializable]
    public class UiContextManager
    {
        public int Depth { get; private set; }
        public UiContextType ContextType => _contextStack.Peek().contextType;

        private Stack<UiContext> _contextStack;
        public bool CanPop => _contextStack.Count > 1;
        public UiContext CurrentContext => _contextStack.Peek();

        public UiContextManager() { _contextStack = new Stack<UiContext>(); }

        public void PushContext(UiContext context)
        {
            // don't add if it is the same context
            if (0 < Depth && CurrentContext.name == context.name)
                return;

            context.SetActive(true);
            context.SetOrder(Depth);
            _contextStack.Push(context);
            Depth++;
        }

        public UiContext PopContext()
        {
            if (!CanPop) return null;

            var context = _contextStack.Pop();
            context.SetOrder(0);
            context.SetActive(false);
            Depth--;

            context = _contextStack.Peek();
            if (context != null)
                context.SetActive(true);

            return context;
        }

        public void PopAll()
        {
            while (CanPop)
                PopContext();
        }
    }
}
