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

public class VitalsMonitoringMenu : MonoBehaviour, IEventListener<AgentChangedEvent>
{
    public static string StatusField = "INJURY: ";

    public GameObject statusText;
    public GameObject leftInfoField;
    public GameObject rightInfoField;

    private Agent _agent;

    private TextMeshProUGUI _statusTextMesh;
    private TextMeshProUGUI _leftInfoFieldTextMesh;
    private TextMeshProUGUI _rightInfoFieldTextMesh;

    void Start()
    {
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
        if (this._agent == null)
            return;

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
