using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

using Animation.Systems;

namespace Animation.Entities.Extractor
{
    [System.Serializable]
    public struct ExtractorAnimationParams : IAnimationParams
    {
        public Transform transform => obj.transform;
        public int Guid => obj.GetInstanceID();

        public float speed;
        public float moveAmount;
        public GameObject obj;

        public Transform Pivot { get; private set; }
        public float TimeAtSpawn { get; private set; }

        public static void Initialize(ref ExtractorAnimationParams eab, float time)
        {
            eab.TimeAtSpawn = time;
            eab.Pivot = eab.obj.transform.parent.transform;
        }
    }

    public class ExtractorAnimationSystem : AnimationSystem<ExtractorAnimationParams>
    {
        public override void OnDestroy()
        {
            // nothing to do
        }

        public override void Update(float time, float deltaTime)
        {
            foreach (ExtractorAnimationParams animated in this.Animated.Values)
            {
                float t = time - animated.TimeAtSpawn;
                float moveAmount = animated.moveAmount * Mathf.Sin(t * animated.speed - animated.TimeAtSpawn);
                var pivot = animated.Pivot.position;
                animated.transform.position = pivot + new Vector3(0, moveAmount);
            }
        }
    }

    public class ExtractorAnimationSystemThreaded : AnimationSystemThreaded<ExtractorAnimationParams>
    {
        private struct ExtractorAnimationJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Vector3> Pivots;
            [ReadOnly] public NativeArray<float> MoveAmounts;
            [ReadOnly] public NativeArray<float> Speeds;
            [ReadOnly] public NativeArray<float> Times;
            public float Time;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = Pivots[index] + new Vector3(0, MoveAmounts[index] * Mathf.Sin(Time * Speeds[index] - Times[index]));
            }
        }

        private int _count = 0;
        private NativeArray<Vector3> _pivots;
        private NativeArray<float> _moveAmounts;
        private NativeArray<float> _speeds;
        private NativeArray<float> _times;
        private TransformAccessArray _transforms;

        public override JobHandle ScheduleAnimationJob(float time, float deltaTime)
        {
            SetupNativeArrays(time);

            var animationJob = new ExtractorAnimationJob()
            {
                Pivots = _pivots,
                MoveAmounts = _moveAmounts,
                Speeds = _speeds,
                Times = _times,
                Time = time
            };

            var jobHandle = animationJob.Schedule(_transforms);

            return jobHandle;
        }

        private void SetupNativeArrays(float time)
        {
            // how many elements to be allocated
            int count = Animated.Count;

            // simple optimization: if count is the same, it was already allocated and nothing needs to be done
            // TODO: do this with event generator -> queue reallocation on Animated changed event
            if (this._count == count)
                return;

            if (this._count > 0)
                DeallocateNativeArrays();

            _pivots = new NativeArray<Vector3>(count, Allocator.Persistent);
            _moveAmounts = new NativeArray<float>(count, Allocator.Persistent);
            _speeds = new NativeArray<float>(count, Allocator.Persistent);
            _times = new NativeArray<float>(count, Allocator.Persistent);
            _transforms = new TransformAccessArray(count, -1);

            this._count = count;

            int i = 0;
            foreach (ExtractorAnimationParams animated in this.Animated.Values)
            {
                _pivots[i] = animated.Pivot.position;
                _moveAmounts[i] = animated.moveAmount;
                _speeds[i] = animated.speed;
                _times[i] = time;
                _transforms.Add(animated.transform);
                i++;
            }
        }

        private void DeallocateNativeArrays()
        {
            _pivots.Dispose();
            _moveAmounts.Dispose();
            _speeds.Dispose();
            _times.Dispose();
            _transforms.Dispose();
        }

        public override void OnDestroy()
        {
            DeallocateNativeArrays();

            base.OnDestroy();
        }
    }
}
