using UnityEngine;
using Utilities.Events;

using Common;

namespace Entities
{
    public abstract class EntityComponentEvent : GameEvent
    {
        public EntityComponentEvent() { }
    }

    public class EntityComponentAddedEvent : GameEvent
    {
        public Entity Entity { get; private set; }
        public EntityComponent EntityComponent { get; private set; }

        public EntityComponentAddedEvent(Entity entity, EntityComponent entityComponent) : base()
        {
            this.Entity = entity;
            this.EntityComponent = entityComponent;
        }
    }

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

        public int Guid => entity.Guid;

        public Entity entity { get; private set; }

        public void Awake()
        {
            this.entity = this.gameObject.GetComponent<Entity>();
        }

        public abstract void OnDestroy();
    }
}
