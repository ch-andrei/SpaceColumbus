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

        protected IMoveComponent MoveComponent = null;
        protected IAttackComponent AttackComponent = null;

        public bool CanMove => MoveComponent.CanMove;
        public bool CanAttack => AttackComponent.CanAttack;

        protected IntelligenceLevel Intelligence;
        protected BehaviourState Behaviour;

        public virtual void Start()
        {
            this.Entity = entity;

            MoveComponent = new MoveComponent(this.GetComponent<NavMeshAgent>());
            AttackComponent = new AttackComponent();
        }

        public void FixedUpdate()
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
