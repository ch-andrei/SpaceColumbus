using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Brains.Attack;
using Brains.Movement;

namespace Brains
{
    [System.Serializable]
    public abstract class AgentBrain
    {
        public GameObject entityObject { get; private set; }

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
            IdleAgressive,
            Moving,
            AttackMoving,
            AttackTargeting,
            AttackEngaging,
        }

        protected MoveBrain MoveBrain = null;
        protected AttackBrain AttackBrain = null;

        protected IntelligenceLevel Intelligence;
        protected BehaviourState Behaviour;

        public AgentBrain(GameObject entityObject, MoveBrain moveBrain, AttackBrain attackBrain)
        {
            this.entityObject = entityObject;
            this.MoveBrain = moveBrain;
            this.AttackBrain = attackBrain;
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
