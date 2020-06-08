using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using Brains.Attack;
using Brains.Movement;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Brains
{
    [System.Serializable]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BrainComponent : EntityComponent
    {
        public Entity Entity { get; private set; }

        public enum IntelligenceLevel
        {
            Primitive,
            Low,
            Moderate,
            Advanced,
        }

        public enum BehaviourState
        {
            Idle,
            IdleRoaming,
            IdleAggressive,
            Moving,
            AttackMoving,
            AttackTargeting,
            AttackEngaging,
        }

        protected MoveBrain MoveBrain = null;
        protected AttackBrain AttackBrain = null;

        protected IntelligenceLevel Intelligence;
        protected BehaviourState Behaviour;

        public virtual void Start()
        {
            this.Entity = entity;

            MoveBrain = new MoveBrain(this.GetComponent<NavMeshAgent>());
            AttackBrain = new AttackBrain();
        }

        public void ProcessTick()
        {
            MakeDecision();
            Act();
        }

        public abstract bool MoveTo(Vector3 destination);

        public abstract void StopMoving();

        protected abstract void MakeDecision();
        protected abstract void Act();
    }
}
