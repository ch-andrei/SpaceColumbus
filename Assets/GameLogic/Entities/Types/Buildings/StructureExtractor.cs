using System;
using System.Collections.Generic;

using UnityEngine;

using Animation;
using Animation.Systems.Extractor;
using UnityEngine.Serialization;

namespace Entities
{
    public class StructureExtractor : Structure
    {
        [FormerlySerializedAs("AnimatedObject")] public GameObject animatedObject;
        [FormerlySerializedAs("AnimationParameters")] public ExtractorAnimationBase animationParameters;

        public override string Name { get { return "Extractor"; } }

        public override void Start()
        {
            base.Start();

            this.animationParameters = new ExtractorAnimationBase(animatedObject, Time.time);

            AnimationManager.RegisterAnimation<ExtractorAnimationSystemThreaded>(this.animationParameters);
        }

        private void OnDestroy()
        {
            AnimationManager.UnregisterAnimation<ExtractorAnimationSystemThreaded>(this.animationParameters);
        }
    }
}
