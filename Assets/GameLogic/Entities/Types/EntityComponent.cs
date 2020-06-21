using UnityEngine;
using Utilities.Events;

using Common;

namespace Entities
{
    public enum EntityComponentType
    {
        Damageable,
        Selectable,
        Movement,
        Brain,
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

        public void Awake()
        {
            this.entity = this.gameObject.GetComponent<Entity>();
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

    public abstract class EntityComponentSystem : INamed
    {
        public abstract string Name { get; }
        public abstract void Update(float time, float deltaTime);
    }
}
