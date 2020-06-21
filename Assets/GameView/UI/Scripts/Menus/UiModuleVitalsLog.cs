using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TMPro;

using Entities;
using Entities.Bodies;
using Entities.Health;

using UI.Components;
using UI.Utils;
using UI.Fields;

using Utilities.Events;

namespace UI.Menus
{
    public struct VitalLogInfo
    {
        public UiFieldVitalsLog Log;
        public GameObject Go;

        public VitalLogInfo(UiFieldVitalsLog log, GameObject go) { this.Log = log; this.Go = go; }
    }

    public class UiModuleVitalsLog : UiModuleWithScrollableItems, IEventListener<DamageableComponentEvent>
    {
        public int numLogsInPool = 20;

        private static string _menuTitle = "VITALS MONITORING";
        private static string _statusField = "INJURY: ";

        public GameObject vitalsLogEntryPrefab;

        private List<VitalLogInfo> _vitalLogs;
        private Entity _entity;
        private DamageableComponent _damageableComponent;
        private int _activeLogs = 0;

        public new void Awake()
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

        public void SetObservedAgent(Entity entity)
        {
            // only update if currently observed agent is not the same as the agent we want to observe
            if (entity is null || entity.Equals(this._entity))
                return;

            this._damageableComponent = EntityManager.GetComponent<DamageableComponent>(entity);

            if (this._damageableComponent is null)
                return;

            this._entity = entity;
            this._damageableComponent.AddListener(this);

            UpdateVitalsLog();
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
            Debug.Log("UpdateVitalsLog");

            titleTextRight.Text = GetStatusString(this._damageableComponent.GetDamageState());

            DeactivateVitalsLog();
            ProcessVitalsLog(_damageableComponent.Body);
        }

        public string GetStatusString(EDamageState damageState)
        {
            return _statusField + HpSystemDamageStates.DamageStateToStrWithColor(damageState);
        }

        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor($"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]", HpSystemDamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void ProcessVitalsLog(Body body)
        {
            foreach (var bodyPart in body.BodyParts)
            {
                if (!bodyPart.IsDamaged)
                    continue;

                if (_activeLogs == this._vitalLogs.Count)
                    AddNewVitalLogToPool();

                var vli = this._vitalLogs[_activeLogs];

                vli.Log.Initialize(bodyPart);
                vli.Go.SetActive(true);

                _activeLogs++;
            }
        }

        public bool OnEvent(DamageableComponentEvent gameComponentEvent)
        {
            bool active = !(this._entity is null) && this._entity.Equals(gameComponentEvent.DamageableComponent);

            if (active)
                UpdateVitalsLog();

            return active;
        }
    }
}
