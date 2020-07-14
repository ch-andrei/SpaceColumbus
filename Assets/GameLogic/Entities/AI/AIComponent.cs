using UnityEngine;

using Entities;
using Brains.Attack;
using Brains.Movement;
using UnityEngine.AI;

namespace Brains
{
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

    [System.Serializable]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AIComponent : EntityComponent
    {
        public Entity Entity { get; private set; }

        protected IMoveComponent MoveComponent = null;
        protected IAttackComponent AttackComponent = null;

        public bool CanMove => MoveComponent.CanMove;
        public bool CanAttack => AttackComponent.CanAttack;

        protected IntelligenceLevel Intelligence;
        protected BehaviourState Behaviour;

        public virtual void Start()
        {
            this.Entity = entity;

            var navMeshComponent = this.GetComponent<NavMeshAgent>();
            MoveComponent = new MoveComponent(navMeshComponent);

            AttackComponent = new AttackComponent();
        }

        public abstract bool MoveTo(Vector3 destination);

        public abstract void StopMoving();

        public abstract void MakeDecision();
    }
}
