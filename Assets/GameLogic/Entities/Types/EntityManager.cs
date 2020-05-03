using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public static class EntityManager
    {
        private static Dictionary<int, Entity> _entities;

        public static void RegisterEntity(Entity entity)
        {
            int id = entity.GetId();
            if (!_entities.ContainsKey(id))
                _entities.Add(id, entity);
        }
        
        public static void UnregisterEntity(Entity entity)
        {
            int id = entity.GetId();
            if (_entities.ContainsKey(id))
                _entities.Remove(id);
        }

        public static void Initialize()
        {
            _entities = new Dictionary<int, Entity>();
        }

        public static void Update()
        {
            // TODO: cluster entities?
        }



    }
}
