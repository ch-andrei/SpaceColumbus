using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Common;

using Entities.Bodies;
using Entities.Damageables;
using EntitySelection;

using Utilities.Events;

namespace Entities
{
    [System.Serializable]
    [RequireComponent(typeof(NavMeshObstacle), typeof(BoxCollider))]
    public class Structure : EntityInitializer
    {
        public override string Name => "Structure";

        public override void InitializeAwake()
        {
            this.Entity.Name = Name;
            this.Entity.EntityType = EntityType.Structure;

            // match collider and navmesh obstacle sizes
            var obstacle = this.GetComponent<NavMeshObstacle>();
            var collider = this.GetComponent<BoxCollider>();
            obstacle.size = collider.size;
        }
    }
}
