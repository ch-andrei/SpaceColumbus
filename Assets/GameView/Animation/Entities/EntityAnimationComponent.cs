using Animation;
using Animation.Systems;

using Entities;

namespace Animation.Entities
{
    public abstract class EntityAnimationComponent : EntityComponent
    {
        public abstract IAnimationParams AnimationParams { get; }

        public void Start()
        {
            AnimationManager.RegisterAnimation(AnimationParams);
        }

        public override void OnDestroy()
        {
            AnimationManager.UnregisterAnimation(AnimationParams);
        }
    }
}
