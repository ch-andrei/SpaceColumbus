using UnityEngine;
using Entities;

namespace Brains
{
    [System.Serializable]
    public class AiComponentModerate : AIComponent
    {
        [Range(0, 1f)] public float nonIdleProbability = 0.005f;
        [Range(0, 1f)] public float idleRoamingProbability = 0.5f;

        [Range(2f, 10f)] public float maxIdleMoveMultiplier = 2f;
        [Range(0f, 100f)] public float maxIdleMoveDistance = 10f;

        private static int _maxPathFindAttempts = 10;

        public override EntityComponentType ComponentType => EntityComponentType.AI;
        public override string Name => "Brain";

        private Animator _animator;
        private bool _hasAnimator;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();

            this.Intelligence = IntelligenceLevel.Moderate;
            this.Behaviour = BehaviourState.Idle;

            this._animator = GetComponentInChildren<Animator>();
            _hasAnimator = !(this._animator is null);

            if (_hasAnimator)
                this._animator.StartPlayback();
        }

        public override void MakeDecision()
        {
            float r1 = UnityEngine.Random.value;
            float r2 = UnityEngine.Random.value;
            float r3 = UnityEngine.Random.value;

            // Agent AI Final State Machine
            if (this.Behaviour == BehaviourState.Idle)
            {
                IdleState(r1);
            }
            else if (this.Behaviour == BehaviourState.Moving)
            {
                if (this.MoveComponent.AtDestination())
                    TransitionIdleState();
            }
            else if (this.Behaviour == BehaviourState.IdleRoaming)
            {
                if (this.MoveComponent.AtDestination())
                    TransitionIdleState();
            }
            else if (this.Behaviour == BehaviourState.IdleAggressive)
            {
                TransitionRoamingState(); // placeholder
            }
            else if (this.Behaviour == BehaviourState.AttackMoving)
            {

            }
            else if (this.Behaviour == BehaviourState.AttackTargeting)
            {

            }
            else if (this.Behaviour == BehaviourState.AttackEngaging)
            {

            }
            else
            {
                this.Behaviour = BehaviourState.Idle;
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
            this.Behaviour = BehaviourState.Idle;

            if (_hasAnimator)
            {
                this._animator.StartPlayback();
            }
        }

        private void TransitionMovingState()
        {
            this.Behaviour = BehaviourState.Moving;

            if (_hasAnimator)
                this._animator.StopPlayback();
        }

        private void TransitionRoamingState()
        {
            this.Behaviour = BehaviourState.IdleRoaming;

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

        public override void OnDestroy()
        {
            // do nothing
        }
    }
}
