using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;

using Common;

namespace Animation.Systems
{
    public interface IAnimationParams : IIdentifiable { }

    public abstract class AnimationSystem : IDestroyable
    {
        private static readonly int AnimationSystemCapacity = 997; // 997 is a prime number

        public Dictionary<int, IAnimationParams> Animated;

        public AnimationSystem() : this(-1) { }

        public AnimationSystem(int capacity)
        {
            if (capacity == -1)
                capacity = AnimationSystemCapacity;

            this.Animated = new Dictionary<int, IAnimationParams>(capacity: capacity);
        }

        public virtual void RegisterAnimationParams(IAnimationParams animationParamsBase)
        {
            int id = animationParamsBase.Guid;

            if (!this.Animated.ContainsKey(id))
                this.Animated.Add(id, animationParamsBase);
            else
                Debug.Log("Warning: Animation System tried to register a component more than once.");
        }

        public void UnregisterAnimationParams(IAnimationParams animationParamsBase)
        {
            UnregisterAnimationParams(animationParamsBase.Guid);
        }

        public void UnregisterAnimationParams(int id)
        {
            if (this.Animated.ContainsKey(id))
                this.Animated.Remove(id);
        }

        public abstract void Update(float time, float deltaTime);

        public abstract void OnDestroy();
    }

    public abstract class AnimationSystem<T> : AnimationSystem
        where T : IAnimationParams
    {
        public override void RegisterAnimationParams(IAnimationParams ab)
        {
            if (ab is T abT)
                AddAnimated(abT);
            else
                Debug.Log("WARNING Animation System: Tried to add animation base with incorrect type.");
        }

        public void AddAnimated(T ap)
        {
            base.RegisterAnimationParams(ap);
        }
    }

    public abstract class AnimationSystemThreaded<T> : AnimationSystem<T> where T : IAnimationParams
    {
        protected JobHandle AnimationJobHandle;

        public abstract JobHandle ScheduleAnimationJob(float time, float deltaTime);

        public override void Update(float time, float deltaTime)
        {
            AnimationJobHandle.Complete(); // wait for previous job to complete

            AnimationJobHandle = ScheduleAnimationJob(time, deltaTime); // schedule next job
        }

        public override void OnDestroy()
        {
            AnimationJobHandle.Complete();
        }
    }
}
