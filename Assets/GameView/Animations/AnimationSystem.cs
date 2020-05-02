using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Unity.Jobs;

using Common;

namespace Animation.Systems
{
    [System.Serializable]
    public abstract class AnimationBase : IIdentifiable
    {
        public GameObject go { get; private set; }

        public AnimationBase(GameObject go)
        {
            this.go = go;
        }

        public int GetId()
        {
            return this.go.GetInstanceID();
        }
    }

    public abstract class AnimationSystem : IDestroyable
    {
        private static readonly int AnimationSystemCapacity = 997; // 997 is a prime number

        public Dictionary<int, AnimationBase> Animated;

        public AnimationSystem() : this(-1) { }

        public AnimationSystem(int capacity)
        {
            if (capacity == -1)
                capacity = AnimationSystemCapacity;

            this.Animated = new Dictionary<int, AnimationBase>(capacity: capacity);
        }

        public virtual void AddAnimated(AnimationBase animationBase)
        {
            int id = animationBase.GetId();

            if (!this.Animated.ContainsKey(id))
                this.Animated.Add(id, animationBase);
        }

        public void RemoveAnimated(AnimationBase animationBase)
        {
            RemoveAnimated(animationBase.GetId());
        }

        public void RemoveAnimated(int id)
        {
            if (this.Animated.ContainsKey(id))
                this.Animated.Remove(id);
        }

        public abstract void Update(float time, float deltaTime);

        public abstract void OnDestroy();
    }

    public abstract class AnimationSystem<T> : AnimationSystem
        where T : AnimationBase
    {
        public override void AddAnimated(AnimationBase ab)
        {
            if (ab is T abT)
                AddAnimated(abT);
            else
                Debug.Log("WARNING Animation System: Tried to add animation base with incorrect type.");
        }

        public void AddAnimated(T ap)
        {
            base.AddAnimated(ap);
        }
    }

    public abstract class AnimationSystemThreaded<T> : AnimationSystem<T>
    where T : AnimationBase
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