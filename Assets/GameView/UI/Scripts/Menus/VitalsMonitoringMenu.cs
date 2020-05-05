using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using Entities.Bodies;
using Entities.Bodies.Health;
using Entities.Bodies.Damages;

using Utilities.Events;

using TMPro;
using UnityEngine.Serialization;

namespace UI.Menus
{
    public class VitalsMonitoringMenu : MonoBehaviour, IEventListener<AgentChangedEvent>
    {
        public static string StatusField = "INJURY: ";

        public GameObject statusText;
        public GameObject leftInfoField;
        public GameObject rightInfoField;

        private Agent _agent = null;
        private bool hasAgent;

        private TextMeshProUGUI _statusTextMesh;
        private TextMeshProUGUI _leftInfoFieldTextMesh;
        private TextMeshProUGUI _rightInfoFieldTextMesh;
        
        void Start()
        {
            hasAgent = false;
            Initialize();
        }

        public void Initialize()
        {
            _statusTextMesh = statusText.GetComponent<TextMeshProUGUI>();
            _leftInfoFieldTextMesh = leftInfoField.GetComponent<TextMeshProUGUI>();
            _rightInfoFieldTextMesh = rightInfoField.GetComponent<TextMeshProUGUI>();
        }

        void UpdateView()
        {
            if (hasAgent)
                _statusTextMesh.text = GetStatusString(this._agent.GetDamageState());
        }

        public string GetStatusString(EDamageState damageState)
        {
            return StatusField + DamageStates.DamageStateToStrWithColor(damageState);
        }

        public void SetObservedAgent(Agent agent)
        {
            if (this._agent != agent)
            {
                this._agent = agent;
                hasAgent = true;
                agent.AddListener(this);
                UpdateView();
            }
        }

        public bool OnEvent(AgentChangedEvent gameEvent)
        {
            bool active = gameEvent.entity == this._agent;

            if (active)
                UpdateView();

            return active;
        }
    }
}