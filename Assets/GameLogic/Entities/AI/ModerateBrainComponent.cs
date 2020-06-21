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

        private Animator _animator;
        private bool _hasAnimator;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();

            this.Intelligence = BrainComponent.IntelligenceLevel.Moderate;
            this.Behaviour = BrainComponent.BehaviourState.Idle;

            this._animator = GetComponentInChildren<Animator>();
            _hasAnimator = !(this._animator is null);

            if (_hasAnimator)
                this._animator.StartPlayback();

        }

        protected override void MakeDecision()
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;
            float r3 = UnityEngine.Random.value;

            // Agent AI Final State Machine
            if (this.Behaviour == BrainComponent.BehaviourState.Idle)
            {
                IdleState(r1);
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.Moving)
            {
                if (this.MoveComponent.AtDestination())
                    TransitionIdleState();
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.IdleRoaming)
            {
                if (this.MoveComponent.AtDestination())
                    TransitionIdleState();
            }
            else if (this.Behaviour == BrainComponent.BehaviourState.IdleAggressive)
            {
                TransitionRoamingState(); // placeholder
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

        private void IdleState(float r)
        {
            // should transition to non-idle?
            if (r < nonIdleProbability)
            {
                // decide where to go
                Vector3 crtPos = this.Entity.Position;
                bool success = false;
                int numAttempt = 0;
                while (!success && numAttempt++ < _maxPathFindAttempts)
                {
                    // find random nearby destination for idling roam
                    var movement = UnityEngine.Random.insideUnitCircle * maxIdleMoveDistance;
                    var destination = crtPos + new Vector3(movement.x, 0, movement.y);

                    success = this.MoveComponent.SetDestination(destination);
                }

                if (success)
                    TransitionRoamingState();
            }
        }

        private void TransitionIdleState()
        {
            this.Behaviour = BrainComponent.BehaviourState.Idle;

            if (_hasAnimator)
            {
                this._animator.StartPlayback();
            }
        }

        private void TransitionMovingState()
        {
            this.Behaviour = BrainComponent.BehaviourState.Moving;

            if (_hasAnimator)
                this._animator.StopPlayback();
        }

        private void TransitionRoamingState()
        {
            this.Behaviour = BrainComponent.BehaviourState.IdleRoaming;
            if (_hasAnimator)
                this._animator.StopPlayback();
        }

        public override bool MoveTo(Vector3 destination)
        {
            bool success = this.MoveComponent.SetDestination(destination);
            if (success)
                TransitionMovingState();
            return success;
        }

        public override void StopMoving()
        {
            this.MoveComponent.StopMoving();
            TransitionIdleState();
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
