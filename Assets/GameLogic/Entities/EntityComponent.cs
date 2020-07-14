using System.Collections.Generic;
using UnityEngine;

using Common;
using Entities.Materials;
using Utilities.XmlReader;
using Utilities.Events;

namespace Entities
{
    public enum EntityComponentType
    {
        Damageable,
        Selectable,
        Movement,
        AI,
        Capacities,
        Animation,
        None
    }

    public abstract class EntityComponent : MonoBehaviour, INamed, IIdentifiable
    {
        public abstract EntityComponentType ComponentType { get; }
        public abstract string Name { get; }

        public Entity entity { get; private set; }

        // id will be set by EntityManager when the entity component is registered
        private int _id;
        public void SetId(int id) { this._id = id; }
        public int Guid => _id;

        public override int GetHashCode() => this.Guid;

        public virtual void Awake()
        {
            this.entity = EntityManager.GetEntityInParent(this.gameObject);
            this.entity.AddComponent(this);
        }

        public abstract void OnDestroy();
    }

    public abstract class EntityComponentEvent : GameEvent
    {
        protected EntityComponent Component;

        public EntityComponentEvent(EntityComponent component)
        {
            this.Component = component;
        }
    }
}
