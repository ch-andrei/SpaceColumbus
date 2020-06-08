using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

using Common;

using EntitySelection;
using Brains;
using Players;
using Entities.Health;
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

    public class EntityEventGenerator : EventGenerator<EntityChangeEvent>, IEventListener<DamageableEvent>
    {
        private Entity _entity;
        public EntityEventGenerator(Entity entity) : base() { this._entity = entity; }

        public bool OnEvent(DamageableEvent bodyEvent)
        {
            this.Notify(new EntityChangeEvent(this._entity));

            return true;
        }
    }

    public class Entity : MonoBehaviour, IWithPosition, IWithPosition2d, INamed, IIdentifiable,
        IWithListeners<EntityChangeEvent>, IEquatable<Entity>
    {
        public EntityType entityType { get; set; }

        public Vector3 Position => this.transform.position;
        public Vector2 Position2d => new Vector2(Position.x, Position.z);
        public int Guid => this.gameObject.GetInstanceID();
        public bool Equals(Entity other) => this.Guid == other?.Guid;

        private string _name;
        public string Name {
            get => _name;
            set => _name = value;
        }

        public OwnershipInfo OwnershipInfo { get; private set; }

        public List<EntityComponent> Components { get; private set; }

        public void AddComponent(EntityComponent component)
        {
            this.Components.Add(component);
        }

        public EntityEventGenerator entityEventSystem;
        public List<IEventListener<EntityChangeEvent>> EventListeners => entityEventSystem.EventListeners;

        public void Awake()
        {
            entityEventSystem = new EntityEventGenerator(this);
            Components = new List<EntityComponent>();
        }

        public void LateStart()
        {
            foreach (var component in EntityManager.GetComponents(this))
                this.AddComponent(component);

            EntityManager.RegisterEntity(this);
        }

        private void OnDestroy()
        {
            // Debug.Log("Entity on destroy!");
            EntityManager.UnregisterEntity(this);
        }

        public void AddListener(IEventListener<EntityChangeEvent> eventListener)
        {
            entityEventSystem.AddListener(eventListener);
        }
    }

    [System.Serializable]
    [RequireComponent(typeof(Entity))]
    public abstract class EntityInitializer : MonoBehaviour, INamed
    {
        protected Entity Entity;

        public abstract string Name { get; }

        public void Start()
        {
            this.Entity = this.GetComponent<Entity>();
            this.Entity.LateStart();

            Initialize();
        }

        public abstract void Initialize();
    }
}
