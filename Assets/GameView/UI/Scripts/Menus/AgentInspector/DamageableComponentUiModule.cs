using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TMPro;

using Entities;
using Entities.Bodies;
using Entities.Capacities;
using Entities.Damageables;

using UI.Components;
using UI.Utils;
using UI.Fields;

using Utilities.Events;

namespace UI.Menus
{
    public struct BodyPartHealthElement
    {
        public BodyPartHealthInfoField Field;
        public GameObject Go;

        public BodyPartHealthElement(BodyPartHealthInfoField field, GameObject go) { this.Field = field; this.Go = go; }
    }

    public class DamageableComponentUiModule : UiModuleWithScrollableItems
    {
        public int numInitialElementsInPool = 0;
        public int numMaxElementsInPool = 20;

        private static string _menuTitle = "VITALS MONITORING";
        private static string _statusField = "INJURY: ";

        public GameObject vitalsLogEntryPrefab;

        private List<BodyPartHealthElement> _bodyPartHealthElements;
        private int _activeElements = 0;

        public DamageableComponent DamageableComponent { get; set; }

        public new void Awake()
        {
            base.Awake();

            titleTextLeft.Text = _menuTitle;
            titleTextRight.Text = GetStatusString(EDamageState.None);

            // generate pool of available vital logs
            _bodyPartHealthElements = new List<BodyPartHealthElement>();

            for (int i = 0; i < numInitialElementsInPool; i++)
                AddNewBodyPartHealthElement();
        }

        private void AddNewBodyPartHealthElement()
        {
             var vitalsLogObj = Instantiate(vitalsLogEntryPrefab, contentRoot.transform, false);
             var uiFieldVitalsLog = vitalsLogObj.GetComponent<BodyPartHealthInfoField>();

             // deactivate object in pool, this will be enabled when needed
             vitalsLogObj.SetActive(false);

             _bodyPartHealthElements.Add(new BodyPartHealthElement(uiFieldVitalsLog, vitalsLogObj));
        }

        public void SetDefaultView()
        {
            InvalidateElements();

            titleTextRight.Text = GetStatusString(EDamageState.None);
        }

        private void InvalidateElements()
        {
            // TODO: check if contentRoot.SetActive(false) is equivalent
            foreach (var element in _bodyPartHealthElements)
            {
                element.Go.SetActive(false);
            }

            // trim the list, destroy the game objects
            if (numMaxElementsInPool < _bodyPartHealthElements.Count)
                for (int i = numMaxElementsInPool; i < _bodyPartHealthElements.Count; i++)
                {
                    // when removing from list, index doesn't change
                    var element = _bodyPartHealthElements[numMaxElementsInPool];
                    Destroy(element.Go);
                    _bodyPartHealthElements.RemoveAt(numMaxElementsInPool);
                }

            _activeElements = 0;
        }

        public void OnNeedUpdate()
        {
            InvalidateElements();

            titleTextRight.Text = GetStatusString(this.DamageableComponent.GetDamageState());

            ProcessVitalsLog(DamageableComponent.Body);
        }

        public string GetStatusString(EDamageState damageState)
        {
            return _statusField + DamageStates.DamageStateToStrWithColor(damageState);
        }

        public string HpSystemToRichString(HpSystem hpSystem)
        {
            return RichStrings.WithColor($"[{hpSystem.HpCurrent}/{hpSystem.HpBase}]", DamageStates.DamageStateToColor(hpSystem.GetDamageState()));
        }

        public void ProcessVitalsLog(Body body)
        {
            foreach (var bodyPart in body.BodyParts)
            {
                if (!bodyPart.IsDamaged)
                    continue;

                // check if enough elements are available
                if (this._bodyPartHealthElements.Count <= _activeElements)
                    AddNewBodyPartHealthElement();

                // get a log
                var vli = this._bodyPartHealthElements[_activeElements];

                vli.Go.SetActive(true);
                vli.Field.Initialize(bodyPart);

                _activeElements++;
            }
        }
    }
}
