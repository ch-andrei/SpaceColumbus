using UnityEngine;

using Entities;
using Brains.Attack;
using Brains.Movement;
using UnityEngine.AI;

namespace Brains
{
    public class AiSystem : EntityComponentSystem
    {
        private const string DamageableSystemName = "AI System";
        public override string Name => DamageableSystemName;

        private const string TimeBetweenUpdatesField = "Timed/AiSystem";
        protected static float _timeBetweenUpdatesAi = SystemsXml.GetFloat(TimeBetweenUpdatesField);

        protected override float TimeBetweenUpdates => _timeBetweenUpdatesAi;

        public AiSystem() : base()
        {
        }

        protected override void Update(float time, float deltaTime)
        {
            var aiComponents = EntityManager.GetComponents<AIComponent>();

            foreach (var aiComponent in aiComponents)
                aiComponent.MakeDecision();
        }
    }
}
