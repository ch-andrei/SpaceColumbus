using System;
using System.Collections.Generic;

using UnityEngine.Serialization;
using UnityEngine;

using Entities;
using Animation;
using Animation.Entities;
using Animation.Entities.Extractor;
using Animation.Systems;

namespace Animation.Entities.Extractor
{
    [System.Serializable]
    public class ExtractorAnimationComponent : EntityAnimationComponent
    {
        public override EntityComponentType ComponentType => EntityComponentType.Animation;
        public override string Name => "Extractor Animation";

        public ExtractorAnimationParams animationParamsParameters; // speed 8f, move amount 0.75f
        public override IAnimationParams AnimationParams => animationParamsParameters;

        public new void Start()
        {
            ExtractorAnimationParams.Initialize(ref animationParamsParameters, Time.time);

            base.Start();
        }
    }
}
