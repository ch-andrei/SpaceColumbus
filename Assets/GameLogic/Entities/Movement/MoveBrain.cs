using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Brains.Movement
{
    public interface IMoveComponent
    {
        bool CanMove { get; }
        float MoveSpeed { get; }
        void SetMoveSpeed(float moveSpeed);
        bool SetDestination(Vector3 destination);
        void StopMoving();
        bool AtDestination();
    }

    public class StationaryMoveComponent : IMoveComponent
    {
        public bool CanMove => false;
        public float MoveSpeed => _moveSpeed;

        private float _moveSpeed = 0; // always zero for Stationary

        public void SetMoveSpeed(float moveSpeed)
        {
            // do nothing
        }

        public bool SetDestination(Vector3 destination)
        {
            // do nothing
            return true;
        }

        public void StopMoving()
        {
            // do nothing
        }

        public bool AtDestination()
        {
            return true;
        }
    }

    [System.Serializable]
    public class MoveComponent : IMoveComponent
    {
        private NavMeshAgent _navMeshAgent;

        public float stuckDistanceThreshold = 0.025f; // minimal distance to destination to consider that destination is reached

        public bool CanMove => true;

        public float MoveSpeed => this._navMeshAgent.speed;
        public void SetMoveSpeed(float moveSpeed) => this._navMeshAgent.speed = moveSpeed;

        #region StuckConfig
        public float stuckTimeout = 2f; // in seconds

        public bool stuck = false;
        public Vector3 posAtStuck;
        public float timeSinceStuck = 0f;
        public float remainingDistance = 0;
        #endregion StuckConfig

        public Vector3 NavMeshPosition => this._navMeshAgent.nextPosition;

        // Start is called before the first frame update
        public MoveComponent(NavMeshAgent navMeshAgent)
        {
            this.posAtStuck = Vector3.positiveInfinity;
            this._navMeshAgent = navMeshAgent;
        }

        public bool SetDestination(Vector3 destination)
        {
            if (!this._navMeshAgent.isOnNavMesh)
                return false;

            NavMeshPath path = new NavMeshPath();
            bool success = this._navMeshAgent.SetDestination(destination);

            remainingDistance = this._navMeshAgent.remainingDistance;

            return success;
        }

        public void StopMoving()
        {
            if (!this._navMeshAgent.isOnNavMesh)
                return;

            float v = this._navMeshAgent.velocity.magnitude;
            float distToStop = v * v / this._navMeshAgent.acceleration / 2f;
            this._navMeshAgent.SetDestination(NavMeshPosition + distToStop * this._navMeshAgent.velocity.normalized);
        }

        // TODO: make sure this works properly
        public void CheckStuck()
        {
            if (this._navMeshAgent.pathPending)
            {
                this.stuck = false;
                return;
            }

            bool stuck = false;
            Vector3 curPos = NavMeshPosition;
            stuck |= (posAtStuck - curPos).magnitude < stuckDistanceThreshold;
            stuck |= (Mathf.Abs(remainingDistance - this._navMeshAgent.remainingDistance)) <= this._navMeshAgent.stoppingDistance;
            if (stuck && !this.stuck) // newly stuck; update position at stuck
                posAtStuck = curPos;
            this.stuck = stuck;
        }

        public bool AtDestination()
        {
            if (!this._navMeshAgent.isOnNavMesh)
                return true;

            CheckStuck();
            if (this.stuck)
            {
                timeSinceStuck += Time.deltaTime;
                if (timeSinceStuck > stuckTimeout)
                    return true;
            }
            else
            {
                timeSinceStuck = 0f;
            }

            remainingDistance = this._navMeshAgent.remainingDistance;

            return this._navMeshAgent.remainingDistance <= this._navMeshAgent.stoppingDistance;
        }
    }
}
