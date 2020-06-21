using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Entities.Health;
using EntitySelection;
using UnityEngine;
using Utilities.QuadTree;

namespace Entities
{
    public static class EntityManager
    {
        // private struct EntityNode : IHasPoint
        // {
        //     public Entity Entity;
        //     public Vector2 Point => Entity.Position2d;
        // }
        //
        // private static PointQuadTree<EntityNode> _entityTree;
        //
        // // TODO 1: refactor this out of EntityManager;
        // // TODO 2: get game session region bounds
        // float t = 1e6f; // some large value for the bound
        // _entityTree = new PointQuadTree<EntityNode>(
        //     new Rectangle(-t, -t, t, t)
        // );

        // mapping between entity component type and IDs; ID is added upon entity registration
        // private static Dictionary<EntityComponentType, HashSet<int>> _entitiesWithComponent;

        // private struct CachedSet<T> where T : IIdentifiable
        // {
        //     public HashSet<T> Items;
        //     public bool Dirty; // indicates whether the set must be re-invalidated
        //
        //     public void Initialize()
        //     {
        //         // TODO: hashset with capacity?
        //         // https://stackoverflow.com/questions/6771917/why-cant-i-preallocate-a-hashsett
        //         Items = new HashSet<T>();
        //     }
        // }

        private static int _entityId = 0;
        private static int NewEntityId => _entityId++;

        private static int _componentId = 0;
        private static int NewComponentId => _componentId++;

        // mapping between IDs and entities; ID is added upon entity registration
        private static Dictionary<int, Entity> _entities;
        private static Dictionary<int, EntityComponent> _components;

        // caching for mappings to provide faster lookup
        private static Dictionary<EntityComponentType, HashSet<Entity>> _cachedEntities;
        private static Dictionary<EntityComponentType, HashSet<EntityComponent>> _cachedComponents;

        private static List<EntityComponentSystem> _systems;

        // this must be called before using EntityManager
        public static void Initialize()
        {
            _entities = new Dictionary<int, Entity>();
            _components = new Dictionary<int, EntityComponent>();

            _cachedEntities = new Dictionary<EntityComponentType, HashSet<Entity>>();
            _cachedComponents = new Dictionary<EntityComponentType, HashSet<EntityComponent>>();

            _systems = new List<EntityComponentSystem>();

            AddEntitySystems();
        }

        private static void AddEntitySystems()
        {
            var damageableSystem = DamageableSystem.GetInstance();
            _systems.Add(damageableSystem);

            // TODO: ADD MORE SYSTEMS HERE
        }

        // should be called at fixed intervals, e.g. by FixedUpdate() somewhere
        public static void Update(float time, float deltaTime)
        {
            foreach (var system in _systems)
                system.Update(time, deltaTime);
        }

        #region Methods for getting entities and components

        // returns compile-time type T for EntityComponentType enum
        public static Type GetTypeForComponentType(EntityComponentType type)
        {
            switch (type)
            {
                case EntityComponentType.Damageable:
                    return typeof(DamageableComponent);
                case EntityComponentType.Selectable:
                    return typeof(SelectableComponent);
                // TODO: remove fall through to default
                case EntityComponentType.Movement:
                case EntityComponentType.None:
                default:
                    throw new ArgumentException("EntityManager: queried EntityComponentType is unsupported.");
            }
        }

        // returns enum EntityComponentType for compile-time type T
        public static EntityComponentType GetTypeForComponentType<T>() where T : EntityComponent
        {
            var type = typeof(T);
            if (type == typeof(DamageableComponent))
                return EntityComponentType.Damageable;
            else if (type == typeof(SelectableComponent))
                return EntityComponentType.Selectable;
            else
                throw new ArgumentException("EntityManager: queried Type T is not a supported type of EntityComponent.");
        }

        #region Methods for getting Entity Components from entities and game objects

        public static Entity GetEntity(GameObject go) => go.GetComponentInParent<Entity>();

        // returns all entity components from a given GameObject or Entity
        public static EntityComponent[] GetComponentsUnity(GameObject go) => GetComponentsUnity(GetEntity(go));
        public static EntityComponent[] GetComponentsUnity(Entity en) => en.gameObject.GetComponents<EntityComponent>();

        // returns all entity components with particular Component Type T from a given GameObject or Entity
        public static List<T> GetComponents<T>(GameObject go) where T : EntityComponent =>
            GetComponents<T>(GetEntity(go));
        public static List<T> GetComponents<T>(Entity en) where T : EntityComponent
        {
            if (en is null)
                return new List<T>();

            var components = new List<T>();
            var eType = GetTypeForComponentType<T>();
            foreach (var component in en.Components)
            {
                if (component.ComponentType == eType)
                    components.Add(component as T);
            }

            return components;
        }

        // returns the first entity component with particular Component Type T from a given GameObject or Entity
        public static T GetComponent<T>(GameObject go) where T : EntityComponent => GetComponent<T>(GetEntity(go));
        public static T GetComponent<T>(Entity entity, bool allowSubtypes = true) where T : EntityComponent
        {
            if (entity is null)
                return null;

            foreach (var component in entity.Components)
            {
                var type = component.GetType();
                if (type == typeof(T) || (allowSubtypes && type.IsSubclassOf(typeof(T))))
                    return component as T;
            }

            return null;
        }

        #endregion Methods for getting Entity Components from entities and game objects

        #region Methods for getting all Entity Components

        // returns a list of entities with component of type EntityComponentType
        public static List<Entity> GetEntitiesWithComponent<T>() where T : EntityComponent
            => GetEntitiesWithComponent(GetTypeForComponentType<T>());
        public static List<Entity> GetEntitiesWithComponent(EntityComponentType type)
        {
            var entities = new List<Entity>();

            if (!_cachedEntities.ContainsKey(type))
                return entities;

            entities.AddRange(_cachedEntities[type]);

            return entities;
        }

        // returns all components with particular Component Type T from all entities
        public static List<T> GetComponents<T>() where T : EntityComponent
        {
            var components = new List<T>();

            var eType = GetTypeForComponentType<T>();

            if (!_cachedComponents.ContainsKey(eType))
                return components;

            foreach (var component in _cachedComponents[eType])
                components.Add(component as T);

            return components;
        }

        #endregion Methods for getting all Entity Components
        #endregion Methods for getting entities and components

        #region Entity and Component registration

        // checks if entity was previously registered
        public static bool IsRegisteredEntity(GameObject go) => IsRegisteredEntity(GetEntity(go));
        public static bool IsRegisteredEntity(Entity entity) => IsRegisteredEntityId(entity.Guid);
        public static bool IsRegisteredEntityId(int id) => _entities.ContainsKey(id);

        // register entity with the manager
        public static void RegisterEntity(Entity entity)
        {
            int id = NewEntityId;
            entity.SetId(id);

            Debug.Log($"Registering new entity with id [{id}]");

            if (!IsRegisteredEntityId(id))
                _entities.Add(id, entity);

            foreach (var component in entity.Components)
            {
                RegisterEntityWithComponent(entity, component);
                RegisterComponent(component);
            }
        }

        public static bool IsRegisteredComponent(EntityComponent component) => IsRegisteredEntityId(component.Guid);
        public static bool IsRegisteredComponentId(int id) => _components.ContainsKey(id);

        private static void RegisterEntityWithComponent(Entity entity, EntityComponent component)
        {
            var componentType = component.ComponentType;

            if (!_cachedEntities.ContainsKey(componentType))
                _cachedEntities.Add(componentType, new HashSet<Entity>());

            // since its a hashset, the item will only be added if its unique
            _cachedEntities[componentType].Add(entity);
        }

        // register component with the manager
        private static void RegisterComponent(EntityComponent component)
        {
            int id = NewComponentId;
            component.SetId(id);

            if (!IsRegisteredComponentId(id))
                _components.Add(id, component);

            var type = component.ComponentType;

            if (!_cachedComponents.ContainsKey(type))
                _cachedComponents[type] = new HashSet<EntityComponent>();

            var set = _cachedComponents[type];
            // since its a hashset, the item will only be added if its unique
            if (!set.Add(component))
                Debug.Log("EntityManager Warning: Tried to register a component that was already registered.");
        }

        // remove a registered entity from the manager
        public static void UnregisterEntity(Entity entity)
        {
            // remove from registered ids
            if (!_entities.Remove(entity.Guid))
                Debug.Log("EntityManager Warning: Tried to unregister an entity that wasn't registered.");

            // unregister each component
            foreach (var component in entity.Components)
            {
                // try remove cached entity
                try
                {
                    if (!_cachedEntities[component.ComponentType].Remove(entity))
                        Debug.Log("EntityManager Warning: Tried to un-cache an entity that wasn't cached.");
                }
                catch (KeyNotFoundException e)
                {
                    Debug.Log("EntityManager Warning: Tried to un-cache an entity with a component type that wasn't cached.");
                }

                UnregisterEntityComponent(entity, component);
            }
        }

        private static void UnregisterEntityComponent(Entity entity, EntityComponent component)
        {
            // remove from registered ids
            if (!_components.Remove(component.Guid))
                Debug.Log("EntityManager Warning: Tried to unregister a component that wasn't registered.");

            // try remove cached component
            try
            {
                if (!_cachedComponents[component.ComponentType].Remove(component))
                    Debug.Log("EntityManager Warning: Tried to un-cache a component that wasn't cached.");
            }
            catch (KeyNotFoundException e)
            {
                Debug.Log("EntityManager Warning: Tried to un-cache a component with a component type that wasn't cached.");
            }
        }

        #endregion Entity and Component registration

        public static void WarnEntityMissing(string name)
        {
            Debug.Log($"EntityManager Warning: Tried adding {name} script to a GameObject with no Entity Script. " +
                      $"Removing {name} script; add Entity script first.");
        }
    }
}
