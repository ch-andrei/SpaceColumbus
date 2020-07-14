using System.Collections.Generic;

using UI.Components;

using Entities;
using Entities.Capacities;
using Entities.Damageables;
using UnityEngine;
using Utilities.Events;

namespace UI.Menus
{
    public class AgentInspectorUiModule : UiModule,
        IEventListener<DamageableComponentEvent>,
        IEventListener<CapacitiesComponentEvent>
    {
        public DamageableComponentUiModule damageableUi;
        public CapacitiesComponentUiModule capacitiesUi;

        private Entity _entity;

        public new void Awake()
        {
            base.Awake();
        }

        public void Start()
        {

        }

        public void SetObservedEntity(Entity entity)
        {
            // only update if currently observed agent is not the same as the agent we want to observe
            if (entity is null || entity.Equals(this._entity))
                return;

            Debug.Log("Updating SetObservedAgent");

            this._entity = entity;

            var damageableComponent = EntityManager.GetComponent<DamageableComponent>(entity);
            var capacitiesComponent = EntityManager.GetComponent<CapacitiesComponent>(entity);

            if (damageableComponent is null)
            {
                damageableUi.DamageableComponent = null;
                damageableUi.SetDefaultView();
            }
            else
            {
                damageableComponent.AddListener(this);
                damageableUi.DamageableComponent = damageableComponent;
                damageableUi.OnNeedUpdate();
            }

            if (capacitiesComponent is null)
            {
                capacitiesUi.CapacitiesComponent = null;
                capacitiesUi.SetDefaultView();
            }
            else
            {
                capacitiesComponent.AddListener(this);
                capacitiesUi.CapacitiesComponent = capacitiesComponent;
                capacitiesUi.OnNeedUpdate();
            }
        }

        public void OnNeedUpdate()
        {
            damageableUi.OnNeedUpdate();
            capacitiesUi.OnNeedUpdate();
        }

        public bool OnEvent(DamageableComponentEvent gameEvent)
        {
            if (!this.gameObject.activeSelf)
                return false;

            bool active = !(this._entity is null) && this._entity.Equals(gameEvent.DamageableComponent.entity);

            // update Capacities and Damageable UI when new damage is applied
            if (active)
                OnNeedUpdate();

            return active;
        }

        public bool OnEvent(CapacitiesComponentEvent gameEvent)
        {
            if (!this.gameObject.activeSelf)
                return false;

            bool active = !(this._entity is null) && this._entity.Equals(gameEvent.CapacitiesComponent.entity);

            // only update capacities
            if (active)
                capacitiesUi.OnNeedUpdate();

            return active;
        }
    }
}
