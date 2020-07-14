using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

using Common;

using EntitySelection;
using Brains;
using Entities.Capacities;
using Players;
using Entities.Damageables;
using UnityEngine.Serialization;
using Utilities.Events;
using Utilities.Misc;
using Tools = Utilities.Misc.Tools;

namespace Entities
{
    public enum EntityType : byte
    {
        Structure,
        Agent
    }

    public class EntityChangeEvent : GameEvent
    {
        public Entity Entity { get; private set; }

        public EntityChangeEvent(Entity entity) { this.Entity = entity; }
    }

    public class EntityEventGenerator : EventGenerator<EntityChangeEvent>, IEventListener<EntityComponentEvent>
    {
        private Entity _entity;
        public EntityEventGenerator(Entity entity) : base() { this._entity = entity; }

        public bool OnEvent(EntityComponentEvent entityComponentEvent)
        {
            var eventType = entityComponentEvent.GetType();
            if (eventType == typeof(DamageableComponentEvent))
            {

            }
            else if (eventType == typeof(CapacitiesComponentEvent))
            {

            }

            this.NotifyListeners(new EntityChangeEvent(this._entity));

            return true;
        }
    }

    public class Entity : MonoBehaviour, IWithPosition, IWithPosition2d, INamed, IIdentifiable,
        IWithListeners<EntityChangeEvent>, IEquatable<Entity>
    {
        public EntityType EntityType { get; set; }

        public Vector3 Position => this.transform.position;
        public Vector2 Position2d => new Vector2(Position.x, Position.z);

        public bool Equals(Entity other) => !(other is null) && this.Guid == other.Guid;
        // public bool Equals(EntityComponent other) => !(other is null) && this.Guid == other.entity.Guid;

        private string _name;
        public string Name {
            get => _name;
            set => _name = value;
        }

        // id will be set by EntityManager when the entity is registered
        private int _id;
        public void SetId(int id) { this._id = id; }
        public int Guid => _id;

        public override int GetHashCode() => this.Guid;

        public OwnershipInfo OwnershipInfo { get; private set; }

        public List<EntityComponent> Components { get; private set; }

        public bool hasDamageableComponent { get; private set; }
        public bool hasCapacitiesComponent { get; private set; }
        public bool hasAiComponent { get; private set; }

        public bool isAgent { get; private set; }
        public bool isStructure { get; private set; }

        public void AddComponent(EntityComponent component)
        {
            if (Components is null)
                Components = new List<EntityComponent>();

            Components.Add(component);
        }

        public EntityEventGenerator entityEventSystem;
        public List<IEventListener<EntityChangeEvent>> EventListeners => entityEventSystem.EventListeners;

        public void Awake()
        {
            entityEventSystem = new EntityEventGenerator(this);
        }

        public void Start()
        {
            hasDamageableComponent = EntityManager.HasComponent<DamageableComponent>(this);
            hasCapacitiesComponent = EntityManager.HasComponent<CapacitiesComponent>(this);
            hasAiComponent = EntityManager.HasComponent<AIComponent>(this);

            isAgent = this.EntityType == EntityType.Agent;
            isStructure = this.EntityType == EntityType.Structure;

            // get a unique ID
            // register entity with EntityManager
            // register entity.components with EntityManager
            EntityManager.RegisterEntity(this);
        }

        private void OnDestroy()
        {
            EntityManager.UnregisterEntity(this);
        }

        public void AddListener(IEventListener<EntityChangeEvent> eventListener)
        {
            entityEventSystem.AddListener(eventListener);
        }
    }

    // EntityInitializer is attached to entities game objects and initializes/configures entity on Start()
    [System.Serializable]
    [RequireComponent(typeof(Entity))]
    public abstract class EntityInitializer : MonoBehaviour, INamed
    {
        protected Entity Entity;

        public abstract string Name { get; }

        public void Awake()
        {
            this.Entity = EntityManager.GetEntityInParent(this.gameObject);

            InitializeAwake();

            Destroy(this); // remove itself after initialization
        }

        public abstract void InitializeAwake();
    }
}
