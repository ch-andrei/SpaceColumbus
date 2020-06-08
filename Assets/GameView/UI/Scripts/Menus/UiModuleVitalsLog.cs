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

    public class UiModuleVitalsLog : UiModuleWithScrollableItems, IEventListener<DamageableEvent>
    {
        public int numLogsInPool = 20;

        private static string _menuTitle = "VITALS MONITORING";
        private static string _statusField = "INJURY: ";

        public GameObject vitalsLogEntryPrefab;

        private List<VitalLogInfo> _vitalLogs;
        private Entity _entity;
        private Damageable _damageable;
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
            if (!(this._entity is null) && !this._entity.Equals(entity))
            {
                this._entity = entity;
                this._damageable = EntityManager.GetComponent<Damageable>(this._entity);

                this._damageable.AddListener(this);

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
            titleTextRight.Text = GetStatusString(this._damageable.GetDamageState());

            DeactivateVitalsLog();
            ProcessVitalsLog(_damageable.Body);
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

        public bool OnEvent(DamageableEvent gameEvent)
        {
            bool active = this._entity.Equals(gameEvent.Entity);

            if (active)
                UpdateVitalsLog();

            return active;
        }
    }
}
