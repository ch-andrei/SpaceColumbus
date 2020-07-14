using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEngine;
using UnityEngine.AI;

using Brains;
using Brains.Movement;
using Brains.Attack;
using Common;
using EntitySelection;

using Entities.Bodies;
using Entities.Damageables;

using Utilities.Events;

namespace Entities
{
    [System.Serializable]
    [RequireComponent(typeof(Entity), typeof(AIComponent), typeof(DamageableComponent))]
    public class Agent : EntityInitializer
    {
        public override string Name => "Agent";

        public override void InitializeAwake()
        {
            this.Entity.Name = Name;
            this.Entity.EntityType = EntityType.Agent;
        }
    }
}

