using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Brains.Movement;
using Brains.Attack;
using UnityEngine.Serialization;

namespace Brains
{
    [System.Serializable]
    public class AgentBrainModerate : AgentBrain
    {
        [Range(0, 1f)] public float nonIdleProbability = 0.005f;
        [Range(0, 1f)] public float idleRoamingProbability = 0.5f;

        [Range(2f, 10f)] public float maxIdleMoveMultiplier = 2f;
        [Range(0f, 100f)] public float maxIdleMoveDistance = 10f;

        private static int _maxPathFindAttempts = 10;

        // Start is called before the first frame update
        public AgentBrainModerate(GameObject entityObject, MoveBrain moveBrain, AttackBrain attackBrain) : base(entityObject, moveBrain, attackBrain)
        {
            this.Intelligence = AgentBrain.IntelligenceLevel.Moderate;
            this.Behaviour = AgentBrain.BehaviourState.Idle;
        }

        protected override void MakeDecision()
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;
            float r3 = UnityEngine.Random.value;

            // Agent AI Final State Machine
            if (this.Behaviour == AgentBrain.BehaviourState.Idle)
            {
                // transition to non-idle
                if (r1 < nonIdleProbability)
                {
                    // decide where to go
                    Vector3 crtPos = this.entityObject.transform.position;
                    bool success = false;
                    int numAttempt = 0;
                    while (!success && numAttempt++ < _maxPathFindAttempts)
                    {
                        var movement = UnityEngine.Random.insideUnitCircle * maxIdleMoveDistance;
                        Vector3 destination = crtPos + new Vector3(movement.x, 0, movement.y);

                        success = this.MoveBrain.SetDestination(destination);
                    }

                    this.Behaviour = AgentBrain.BehaviourState.IdleRoaming;
                }
            }
            else if (this.Behaviour == AgentBrain.BehaviourState.Moving)
            {
                if (this.MoveBrain.AtDestination())
                    this.Behaviour = AgentBrain.BehaviourState.Idle;
            }
            else if (this.Behaviour == AgentBrain.BehaviourState.IdleRoaming)
            {
                if (this.MoveBrain.AtDestination())
                    this.Behaviour = AgentBrain.BehaviourState.Idle;
            }
            else if (this.Behaviour == AgentBrain.BehaviourState.IdleAgressive)
            {
                this.Behaviour = AgentBrain.BehaviourState.IdleRoaming; // placeholder
            }
            else if (this.Behaviour == AgentBrain.BehaviourState.AttackMoving)
            {

            }
            else if (this.Behaviour == AgentBrain.BehaviourState.AttackTargeting)
            {

            }
            else if (this.Behaviour == AgentBrain.BehaviourState.AttackEngaging)
            {

            }
            else
            {
                this.Behaviour = AgentBrain.BehaviourState.Idle;
            }
        }

        public override bool MoveTo(Vector3 destination)
        {
            bool success = this.MoveBrain.SetDestination(destination);
            if (success)
                this.Behaviour = AgentBrain.BehaviourState.Moving;
            return success;
        }

        public override void StopMoving()
        {
            this.MoveBrain.StopMoving();
            this.Behaviour = AgentBrain.BehaviourState.Idle;
        }

        protected override void Act()
        {

        }
    }
}
