using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    private static Dictionary<int, Entity> _entities;
    private static Dictionary<EntityComponentType, HashSet<int>> _entitiesWithComponent;

    // this must be called before using EntityManager
    public static void Initialize()
    {
        _entities = new Dictionary<int, Entity>();
        _entitiesWithComponent = new Dictionary<EntityComponentType, HashSet<int>>();

        Debug.Log("Initializing Entity MANAGER");

        // // TODO 1: refactor this out of EntityManager;
        // // TODO 2: get game session region bounds
        // float t = 1e6f; // some large value for the bound
        // _entityTree = new PointQuadTree<EntityNode>(
        //     new Rectangle(-t, -t, t, t)
        // );
    }

    public static Type GetTypeForComponentType(EntityComponentType type)
    {
        switch (type)
        {
            case EntityComponentType.Damageable:
                return typeof(Damageable);
            case EntityComponentType.Selectable:
                return typeof(Selectable);
            // TODO: remove fall through to default
            case EntityComponentType.Movement:
            case EntityComponentType.None:
            default:
                return null;
        }
    }

    public static EntityComponentType GetTypeForComponentType<T>() where T : EntityComponent
    {
        var type = typeof(T);
        if (type == typeof(Damageable))
            return EntityComponentType.Damageable;
        else if (type == typeof(Selectable))
            return EntityComponentType.Selectable;
        else
            return EntityComponentType.None;
    }

    public static List<Entity> GetEntitiesWithComponent<T>() where T : EntityComponent
        => GetEntitiesWithComponent(GetTypeForComponentType<T>());

    public static List<Entity> GetEntitiesWithComponent(EntityComponentType type)
    {
        var entities = new List<Entity>();

        if (!_entitiesWithComponent.ContainsKey(type))
            return entities;

        foreach (int id in _entitiesWithComponent[type])
        {
            entities.Add(_entities[id]);
        }

        return entities;
    }

    public static List<T> GetComponents<T>() where T : EntityComponent
    {
        var components = new List<T>();

        var entities = GetEntitiesWithComponent<T>();

        foreach (var entity in entities)
        {
            components.AddRange(GetComponents<T>(entity));
        }

        return components;
    }

    public static Entity GetEntity(GameObject go) => go.GetComponentInParent<Entity>();

    public static EntityComponent[] GetComponents(GameObject go) => GetComponents(GetEntity(go));
    public static EntityComponent[] GetComponents(Entity en) => en.gameObject.GetComponents<EntityComponent>();

    public static List<T> GetComponents<T>(GameObject go) where T : EntityComponent => GetComponents<T>(GetEntity(go));
    public static List<T> GetComponents<T>(Entity en) where T : EntityComponent
    {
        var components = new List<T>();

        if (en is null)
            return components;

        foreach (var component in en.Components)
        {
            if (component.GetType() == typeof(T))
                components.Add(component as T);
        }

        return components;
    }

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

    public static bool IsRegisteredEntity(Entity entity) => IsRegisteredId(entity.Guid);
    public static bool IsRegisteredId(int id) => _entities.ContainsKey(id);

    public static void RegisterEntity(Entity entity)
    {
        int id = entity.Guid;

        if (!IsRegisteredId(id))
            _entities.Add(id, entity);

        foreach (var component in entity.Components)
            RegisterComponent(component);
    }

    public static void RegisterComponent(EntityComponent component)
    {
        var type = component.ComponentType;
        int id = component.Guid;

        if (!_entitiesWithComponent.ContainsKey(type))
            _entitiesWithComponent.Add(type, new HashSet<int>());

        if (!_entitiesWithComponent[type].Add(id))
            Debug.Log("Warning: Tried to register a component that was already registered.");
    }

    public static void UnregisterEntity(Entity entity)
    {
        int id = entity.Guid;

        if (IsRegisteredId(id))
            _entities.Remove(id);
        else
            Debug.Log("Warning: Tried to unregister an entity that wasn't registered.");

        UnregisterEntityComponents(entity.Components);
    }

    private static void UnregisterEntityComponents(List<EntityComponent> components)
    {
        foreach (var component in components)
            UnregisterEntityComponent(component);
    }

    private static void UnregisterEntityComponent(EntityComponent component)
    {
        if (!_entitiesWithComponent[component.ComponentType].Remove(component.Guid))
            Debug.Log("Warning: Tried to unregister component that wasn't registered.");
    }

    public static void WarnEntityMissing(string name)
    {
        Debug.Log($"Warning: Tried adding {name} script to a GameObject with no Entity Script. " +
                  $"Removing {name} script; add Entity script first.");
    }

    // private static void Reinvalidate()
    // {
    //     // foreach
    // }
    //
    // public static void Update()
    // {
    //     // TODO: cluster entities?
    // }
    //
    // public static List<Entity> GetClosestEntity(int n = 1)
    // {
    //     List<Entity> entities = new List<Entity>();
    //
    //     // TODO: get
    //
    //     return entities;
    // }
}
}
