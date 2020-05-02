using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using Entities;
using Entities.Bodies;
using Entities.Bodies.Health;
using Entities.Bodies.Damages;

using Utilities.Events;

using UI.Utils;
using UnityEngine.Serialization;

namespace UI.Menus
{
    public struct VitalLogInfo
    {
        public UiFieldVitalsLog Log;
        public GameObject Go;

        public VitalLogInfo(UiFieldVitalsLog log, GameObject go) { this.Log = log; this.Go = go; }
    }

    public class UiVitalsLog : UiWithScrollableItems, IEventListener<AgentChangedEvent>
    {
        [FormerlySerializedAs("NumLogsInPool")] public int numLogsInPool = 20;

        private static string _menuTitle = "VITALS MONITORING";
        private static string _statusField = "INJURY: ";

        [FormerlySerializedAs("VitalsLogEntryPrefab")] public GameObject vitalsLogEntryPrefab;

        private List<VitalLogInfo> _vitalLogs;
        private Agent _agent;
        private int _activeLogs = 0;

        public override void Awake()
        {
            base.Awake();

            titleTextLeft.Text = _menuTitle;
            titleTextRight.Text = GetStatusString(EDamageState.None);
        }

        public void Start()
        {
            // generate pool of available vital logs
            _vitalLogs = new List<VitalLogInfo>();
            for (int i = 0; i < numLogsInPool; i++)
                AddNewVitalLogToPool();
        }

        private void AddNewVitalLogToPool()
        {
             var vitalsLogObj = Instantiate(vitalsLogEntryPrefab);
             var uiFieldVitalsLog = vitalsLogObj.GetComponent<UiFieldVitalsLog>();

             vitalsLogObj.transform.SetParent(contentRoot.transform, false);
             vitalsLogObj.SetActive(false);

             _vitalLogs.Add(new VitalLogInfo(uiFieldVitalsLog, vitalsLogObj));
        }

        public void SetObservedAgent(Agent agent)
        {
            // only update if currently observed agent is not the same as the agent we want to observe
            if (this._agent != agent)
            {
                this._agent = agent;

                agent.AddListener(this);

                UpdateVitalsLog();
            }
        }

        void DeactivateVitalsLog()
        {
            foreach (var vli in _vitalLogs)
            {
                vli.Go.SetActive(false);
            }

            _activeLogs = 0;
        }

        void UpdateVitalsLog()
        {
            titleTextRight.Text = GetStatusString(this._agent.GetDamageState());

            DeactivateVitalsLog();
            ProcessVitalsLog(_agent.Body);
        }

        public string GetStatusString(EDamageState damageState)
        {
            return _statusField + DamageStates.DamageStateToStrWithColor(damageState);
        }

        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor($"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]", DamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void ProcessVitalsLog(BodyPart bodyPart, int depth=0)
        {
            if (bodyPart.HasHpSystem && bodyPart.IsDamaged)
            {
                if (_activeLogs == this._vitalLogs.Count)
                    AddNewVitalLogToPool();

                var vli = this._vitalLogs[_activeLogs];

                vli.Log.Initialize(bodyPart);
                vli.Go.SetActive(true);

                _activeLogs++;
            }

            if (bodyPart is BodyPartContainer bpc)
                foreach (var bp in bpc.BodyParts)
                    ProcessVitalsLog(bp, depth + 1);
        }

        public bool OnEvent(AgentChangedEvent gameEvent)
        {
            bool active = gameEvent.entity == this._agent;

            if (active)
                UpdateVitalsLog();

            return active;
        }
    }
}