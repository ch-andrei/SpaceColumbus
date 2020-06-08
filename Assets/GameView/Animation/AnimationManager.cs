using System;
using System.Collections.Generic;
using Animation.Entities.Extractor;
using UnityEngine;

using Entities;
using Animation.Systems;
using Animation.Entities;
using Common;

namespace Animation
{
    public static class AnimationManager
    {
        private static Dictionary<Type, AnimationSystem> _animationSystems = new Dictionary<Type, AnimationSystem>();

        public static AnimationSystem GetAnimationSystemForAnimationBase(IAnimationParams ab)
            => GetAnimationSystemForAnimationBase(ab.GetType());

        private static AnimationSystem GetAnimationSystemForAnimationBase(Type animationBaseType)
        {
            if (animationBaseType == typeof(ExtractorAnimationParams))
                return GetOrCreateAnimationSystem<ExtractorAnimationSystemThreaded>();
            else
                throw new NotImplementedException("Unsupported AnimationBase type.");
        }

        public static T GetOrCreateAnimationSystem<T>() where T : AnimationSystem, new()
        {
            var type = typeof(T);

            AnimationSystemTypeCheck(type);

            if (!_animationSystems.ContainsKey(type))
                _animationSystems[type] = new T();

            return _animationSystems[type] as T;
        }

        public static void AnimationSystemTypeCheck(Type type)
        {
            // check for supported Animation Systems here

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
            foreach (var animationSystem in _animationSystems.Values)
            {
                animationSystem.Update(time, deltaTime);
            }
        }

        public static void RegisterAnimatedComponent(EntityAnimationComponent eac)
        {

        }

        public static void RegisterAnimation(IAnimationParams ab)
        {
            AnimationSystem animator = GetAnimationSystemForAnimationBase(ab);

            if (animator != null)
                animator.AddAnimated(ab);
        }

        public static void UnregisterAnimation(IAnimationParams ab)
        {
            AnimationSystem animator = GetAnimationSystemForAnimationBase(ab);

            if (animator != null)
                animator.RemoveAnimated(ab);
        }

        public static void OnDestroy()
        {
            foreach (var animationSystem in _animationSystems.Values)
            {
                animationSystem.OnDestroy();
            }
        }
    }
}
