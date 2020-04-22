using System;
using System.Collections.Generic;

using UnityEngine;

using Animation;
using Animation.Systems.Extractor;

namespace Entities
{
    public class StructureExtractor : Structure
    {
        public GameObject AnimatedObject;
        public ExtractorAnimationBase AnimationParameters;

        override public string Name { get { return "Extractor"; } }

        override public void Start()
        {
            base.Start();

            this.AnimationParameters = new ExtractorAnimationBase(AnimatedObject, Time.time);

            AnimationManager.RegisterAnimation<ExtractorAnimationSystemThreaded>(this.AnimationParameters);
        }

        private void OnDestroy()
        {
            AnimationManager.UnregisterAnimation<ExtractorAnimationSystemThreaded>(this.AnimationParameters);
        }
    }
}
