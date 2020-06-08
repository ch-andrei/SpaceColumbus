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
using Entities.Health;

using Utilities.Events;

namespace Entities
{
    [System.Serializable]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Agent : EntityInitializer
    {
        public override string Name => "Agent";

        private BrainComponent _brainComponent;

        public override void Initialize()
        {
            this.Entity.Name = Name;
            this.Entity.entityType = EntityType.Agent;

            _brainComponent = EntityManager.GetComponent<ModerateBrainComponent>(this.Entity);
        }

        public void MoveTo(Vector3 destination)
        {
            this._brainComponent.MoveTo(destination);
        }

        public void Stop() {
            this._brainComponent.StopMoving();
        }

        // TODO: remove
        void FixedUpdate()
        {
            // if (UnityEngine.Random.value < 0.005f)
            // {
            //     this.TakeDamage(new Damage(DamageType.Blunt, 5, 0.1f));
            // }
            // AI
            _brainComponent.ProcessTick(); // TODO: convert to brain system
        }
    }
}

