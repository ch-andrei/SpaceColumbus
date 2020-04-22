using System;
using System.Collections.Generic;

using UnityEngine;

using Entities;
using Animation.Systems;
using Animation.Systems.Extractor;

using Common;

namespace Animation
{
    public static class AnimationManager
    {
        private static Dictionary<Type, AnimationSystem> AnimationSystems = new Dictionary<Type, AnimationSystem>();

        public static T GetOrCreateAnimationSystem<T>() where T : AnimationSystem, new()
        {
            Type type = typeof(T);

            AnimationSystemTypeCheck(type);

            if (!AnimationSystems.ContainsKey(type))
                AnimationSystems[type] = new T();

            return AnimationSystems[type] as T;
        }

        public static void AnimationSystemTypeCheck(Type type)
        {
            if (type == typeof(ExtractorAnimationSystem)) { }
            else if (type == typeof(ExtractorAnimationSystemThreaded)) { }

            /*
             * else if (type == typeof(ExtractorAnimationSystem)) { }
             * 
             * add other systems type checking here
             */

            else
            {
                throw new NotImplementedException("Unsupported Animation System.");
            }
        }

        public static void Update(float time, float deltaTime)
        {
            foreach (var animationSystem in AnimationSystems.Values)
            {
                animationSystem.Update(time, deltaTime);
            }
        }

        public static void RegisterAnimation<T>(AnimationBase ab) where T : AnimationSystem, new()
        {
            AnimationSystem animator = GetOrCreateAnimationSystem<T>();

            if (animator != null)
                animator.AddAnimated(ab);
        }

        public static void UnregisterAnimation<T>(AnimationBase ab) where T : AnimationSystem, new()
        {
            AnimationSystem animator = GetOrCreateAnimationSystem<T>();

            if (animator != null)
                animator.RemoveAnimated(ab);
        }

        public static void OnDestroy()
        {
            foreach (var animationSystem in AnimationSystems.Values)
            {
                animationSystem.OnDestroy();
            }
        }
    }
}
