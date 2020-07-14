using System.Collections.Generic;

using Entities.Capacities;
using Entities.Damageables;

using Brains;

namespace Entities
{
    public static class EntitySystemManager
    {
        private static List<EntityComponentSystem> _systems;

        // quick getters
        public static DamageableSystem DamageableSystem { get; private set; }
        public static CapacitiesSystem CapacitiesSystem { get; private set; }
        public static AiSystem AiSystem { get; private set; }

        public static void Initialize()
        {
            DamageableSystem = new DamageableSystem();
            CapacitiesSystem = new CapacitiesSystem();
            AiSystem = new AiSystem();

            _systems = new List<EntityComponentSystem>();
            _systems.Add(DamageableSystem);
            _systems.Add(CapacitiesSystem);
            _systems.Add(AiSystem);
        }

        // should be called at fixed intervals, e.g. by FixedUpdate() somewhere
        public static void Update(float time, float deltaTime)
        {
            // each system handles its own timing
            foreach (var system in _systems)
                system.UpdateTimed(time, deltaTime);
        }
    }
}
