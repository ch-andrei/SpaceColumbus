using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Common;

using Entities.Bodies;
using Entities.Health;
using EntitySelection;

using Utilities.Events;

namespace Entities
{
    [System.Serializable]
    [RequireComponent(typeof(NavMeshObstacle), typeof(BoxCollider))]
    public class Structure : EntityInitializer
    {
        public override string Name => "Structure";

        public override void Initialize()
        {
            this.Entity.Name = Name;
            this.Entity.entityType = EntityType.Structure;
        }

        public void AddListener(IEventListener<EntityChangeEvent> eventListener)
        {
            throw new System.NotImplementedException();
        }
    }
}
