using UnityEngine;

using System;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Animation.Systems.Extractor
{
    [System.Serializable]
    public class ExtractorAnimationBase : AnimationBase
    {
        public float Speed = 8f;
        public float MoveAmount = 0.7f;

        public Transform Pivot { get; private set; }
        public float TimeAtSpawn { get; private set; }

        public ExtractorAnimationBase(GameObject go, float time) : base(go)
        {
            this.TimeAtSpawn = time;
            this.Pivot = this.go.transform.parent.transform;
        }
    }

    public class ExtractorAnimationSystem : AnimationSystem<ExtractorAnimationBase>
    {
        public override void OnDestroy()
        {
            // nothing to do
        }

        public override void Update(float time, float deltaTime)
        {
            foreach (ExtractorAnimationBase animated in this.Animated.Values)
            {
                float t = time - animated.TimeAtSpawn;
                float moveAmount = animated.MoveAmount * Mathf.Sin(t * animated.Speed - animated.TimeAtSpawn);
                var pivot = animated.Pivot.position;
                animated.go.transform.position = pivot + new Vector3(0, moveAmount);
            }
        }
    }

    public class ExtractorAnimationSystemThreaded : AnimationSystemThreaded<ExtractorAnimationBase>
    {
        struct ExtractorAnimationJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Vector3> pivots;
            [ReadOnly] public NativeArray<float> moveAmounts;
            [ReadOnly] public NativeArray<float> speeds;
            [ReadOnly] public NativeArray<float> times;
            public float time;

            public void Execute(int index, TransformAccess transform)
            {
                transform.position = pivots[index] + new Vector3(0, moveAmounts[index] * Mathf.Sin(time * speeds[index] - times[index]));
            }
        }

        int count = 0;
        NativeArray<Vector3> pivots;
        NativeArray<float> moveAmounts;
        NativeArray<float> speeds;
        NativeArray<float> times;
        TransformAccessArray transforms;

        override public JobHandle ScheduleAnimationJob(float time, float deltaTime)
        {
            SetupNativeArrays(time);

            var animationJob = new ExtractorAnimationJob()
            {
                pivots = pivots,
                moveAmounts = moveAmounts,
                speeds = speeds,
                times = times,
                time = time
            };

            var jobHandle = animationJob.Schedule(transforms);

            return jobHandle;
        }

        private void SetupNativeArrays(float time)
        {
            // how many elements to be allocated
            int count = Animated.Count;

            // simple optimization: if count is the same, it was already allocated and nothing needs to be done
            // TODO: do this with event generator -> queue reallocation on Animated changed event
            if (this.count == count)
                return;

            if (this.count > 0)
                DeallocateNativeArrays();

            pivots = new NativeArray<Vector3>(count, Allocator.Persistent);
            moveAmounts = new NativeArray<float>(count, Allocator.Persistent);
            speeds = new NativeArray<float>(count, Allocator.Persistent);
            times = new NativeArray<float>(count, Allocator.Persistent);
            transforms = new TransformAccessArray(count, -1);

            this.count = count;

            int i = 0;
            foreach (ExtractorAnimationBase animated in this.Animated.Values)
            {
                pivots[i] = animated.Pivot.position;
                moveAmounts[i] = animated.MoveAmount;
                speeds[i] = animated.Speed;
                times[i] = time;
                transforms.Add(animated.go.transform);
                i++;
            }
        }

        private void DeallocateNativeArrays()
        {
            pivots.Dispose();
            moveAmounts.Dispose();
            speeds.Dispose();
            times.Dispose();
            transforms.Dispose();
        }

        override public void OnDestroy()
        {
            DeallocateNativeArrays();

            base.OnDestroy();
        }
    }
}
