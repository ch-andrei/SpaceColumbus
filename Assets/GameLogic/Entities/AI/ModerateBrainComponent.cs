using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Entities;
using Brains.Movement;
using Brains.Attack;
using UnityEngine.Serialization;

namespace Brains
{
    [System.Serializable]
    public class ModerateBrainComponent : BrainComponent
    {
        [Range(0, 1f)] public float nonIdleProbability = 0.005f;
        [Range(0, 1f)] public float idleRoamingProbability = 0.5f;

        [Range(2f, 10f)] public float maxIdleMoveMultiplier = 2f;
        [Range(0f, 100f)] public float maxIdleMoveDistance = 10f;

        private static int _maxPathFindAttempts = 10;

        public override EntityComponentType ComponentType => EntityComponentType.Brain;
        public override string Name => "Brain";

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();

            this.Intelligence = BrainComponent.IntelligenceLevel.Moderate;
            this.Behaviour = BrainComponent.BehaviourState.Idle;
        }

        protected override void MakeDecision()
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;
            float r3 = UnityEngine.Random.value;

            // Agent AI Final State Machine
            if (this.Behaviour == BrainComponent.BehaviourState.Idle)
            {
                // transition to non-idle
                if (r1 < nonIdleProbability)
                {
                    // decide where to go
                    Vector3 crtPos = this.Entity.Position;
                    bool success = false;
                    int numAttempt = 0;
                    while (!success && numAttempt++ < _maxPathFindAttempts)
                    {
                        var movement = UnityEngine.Random.insideUnitCircle * maxIdleMoveDistance;
                        Vector3 destination = crtPos + new Vector3(movement.x, 0, movement.y);

                        success = this.MoveBrain.SetDestination(destination);
                    }

                    this.Behaviour = BrainComponent.BehaviourState.IdleRoaming;
                }
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.Moving)
            {
                if (this.MoveBrain.AtDestination())
                    this.Behaviour = BrainComponent.BehaviourState.Idle;
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.IdleRoaming)
            {
                if (this.MoveBrain.AtDestination())
                    this.Behaviour = BrainComponent.BehaviourState.Idle;
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.IdleAggressive)
            {
                this.Behaviour = BrainComponent.BehaviourState.IdleRoaming; // placeholder
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.AttackMoving)
            {

            }
            else if (this.Behaviour == BrainComponent.BehaviourState.AttackTargeting)
            {

            }
            else if (this.Behaviour == BrainComponent.BehaviourState.AttackEngaging)
            {

            }
            else
            {
                this.Behaviour = BrainComponent.BehaviourState.Idle;
            }
        }

        public override bool MoveTo(Vector3 destination)
        {
            bool success = this.MoveBrain.SetDestination(destination);
            if (success)
                this.Behaviour = BrainComponent.BehaviourState.Moving;
            return success;
        }

        public override void StopMoving()
        {
            this.MoveBrain.StopMoving();
            this.Behaviour = BrainComponent.BehaviourState.Idle;
        }

        protected override void Act()
        {

        }

        public override void OnDestroy()
        {
            // do nothing
        }
    }
}
