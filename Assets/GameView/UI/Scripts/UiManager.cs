﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using UnityEngine.Serialization;
using Utilities.Events;

namespace UI.Menus
{
    public class AgentUiActive : UiEvent
    {
        public bool ActiveState { get; private set; }

        public AgentUiActive(bool state)
        {
            this.ActiveState = state;
        }
    }

    public class SelectedEntityEvent : UiEvent
    {
        public Entity Entity;

        public SelectedEntityEvent(Entity entity) { this.Entity = entity; }
    }

    public abstract class UiEvent : GameEvent
    {

    }

    [System.Serializable]
    public struct UiComponent
    {
        public GameObject obj;
        public bool active;
    }

    public class UiManager : MonoBehaviour, IEventListener<UiEvent>
    {
        //public UiComponent EntityUi;
        public UiComponent agentVitalsUi;

        public GameObject vitalsMonitoring;
        UiVitalsLog _vitalsMenu;

        [FormerlySerializedAs("_contextManager")] public UiContextManager contextManager; 
        
        // Start is called before the first frame update
        void Start()
        {
            // contextManager = new UiContextManager();
            
            _vitalsMenu = vitalsMonitoring.GetComponent<UiVitalsLog>();

            OnEvent(new AgentUiActive(true)); // enable, to make sure that it can be disabled
            OnEvent(new AgentUiActive(false)); // disable
        }

        public bool OnEvent(UiEvent gameEvent)
        {
            if (gameEvent is AgentUiActive activeStateEvent)
            {
                if (this.agentVitalsUi.active != activeStateEvent.ActiveState)
                {
                    this.agentVitalsUi.active = activeStateEvent.ActiveState;
                    this.agentVitalsUi.obj.SetActive(activeStateEvent.ActiveState);
                }
            }
            else if (gameEvent is SelectedEntityEvent entityEvent)
            {
                if (entityEvent.Entity is Agent agent)
                    this._vitalsMenu.SetObservedAgent(agent);
                else
                    Debug.Log("Selected something that isn't an agent.");
            }

            return true;
        }

    }

}
