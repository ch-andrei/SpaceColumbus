using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Brains.Movement
{
    [System.Serializable]
    public class MoveBrain
    {
        private NavMeshAgent _navMeshAgent;

        [FormerlySerializedAs("StuckDistanceThreshold")] public float stuckDistanceThreshold = 0.05f; // minimal distance to destination to consider that destination is reached

        #region StuckConfig
        public float stuckTimeout = 2f; // in seconds

        public bool stuck = false;
        public Vector3 posAtStuck;
        public float timeSinceStuck = 0f;
        public float remainingDistance = 0;
        #endregion StuckConfig

        public Vector3 position { get { return this._navMeshAgent.nextPosition; } }

        // Start is called before the first frame update
        public MoveBrain(NavMeshAgent navMeshAgent)
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
            this._navMeshAgent.SetDestination(position + distToStop * this._navMeshAgent.velocity.normalized);
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
            Vector3 curPos = position;
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
