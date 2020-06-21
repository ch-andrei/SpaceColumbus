using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.Serialization;

using Entities;
using Entities.Bodies;
using Entities.Health;

using Utilities.Events;

namespace UI.Menus
{
    public class VitalsMonitoringMenu : MonoBehaviour, IEventListener<EntityChangeEvent>
    {
        public static string StatusField = "INJURY: ";

        public GameObject statusText;
        public GameObject leftInfoField;
        public GameObject rightInfoField;

        private Entity _entity = null;
        private DamageableComponent _damageableComponent = null;
        private bool _hasDamageable;

        private TextMeshProUGUI _statusTextMesh;
        private TextMeshProUGUI _leftInfoFieldTextMesh;
        private TextMeshProUGUI _rightInfoFieldTextMesh;

        void Start()
        {
            _hasDamageable = false;
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
            if (_hasDamageable)
                _statusTextMesh.text = GetStatusString(this._damageableComponent.GetDamageState());
        }

        public string GetStatusString(EDamageState damageState)
        {
            return StatusField + HpSystemDamageStates.DamageStateToStrWithColor(damageState);
        }

        public void SetObservedAgent(Entity entity)
        {
            if (!this._entity.Equals(entity))
            {
                this._entity = entity;
                this._damageableComponent = EntityManager.GetComponent<DamageableComponent>(_entity);

                _hasDamageable = _damageableComponent != null;
                if (_hasDamageable)
                {
                    entity.AddListener(this);
                    UpdateView();
                }
            }
        }

        public bool OnEvent(EntityChangeEvent gameEvent)
        {
            bool active = gameEvent.Entity == this._entity;

            if (active)
                UpdateView();

            return active;
        }
    }
}
