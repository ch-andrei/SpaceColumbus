using System.Collections.Generic;

using UnityEngine;

using Entities.Bodies;
using Entities.Capacities;

namespace Entities.Damageables
{
    public class DamageableSystemEvent : EntityComponentSystemEvent
    {
        public DamageableComponent Damageable;

        public DamageableSystemEvent(DamageableComponent damageable)
        {
            this.Damageable = damageable;
        }
    }

    public class DamageableSystemDamageEvent : DamageableSystemEvent
    {
        public Damage Damage;

        public DamageableSystemDamageEvent(DamageableComponent damageable, Damage damage) : base(damageable)
        {
            this.Damage = damage;
        }
    }

    public class DamageableSystemCapacitiesEvent : DamageableSystemEvent
    {
        public CapacitiesComponent Capacities => this.Damageable.CapacitiesComponent;

        public DamageableSystemCapacitiesEvent(DamageableComponent damageable)
            : base(damageable)
        {
        }
    }

    public class DamageableSystem : EntityComponentSystem<DamageableSystemEvent>
    {
        private const string DamageableSystemName = "Damageable System";
        public override string Name => DamageableSystemName;

        private const string TimeBetweenUpdatesField = "Timed/DamageableSystem";
        protected static float _timeBetweenUpdatesDamageable = SystemsXml.GetFloat(TimeBetweenUpdatesField);

        protected override float TimeBetweenUpdates => _timeBetweenUpdatesDamageable;

        public DamageableSystem() : base()
        {
        }

        public void OnBodyDamageEvent(BodyDamageEvent dmgEvent, Damage damage)
        {
            if (0 < VerbosityLevel)
            {
                foreach (var bodyPartHpEvent in dmgEvent.BodyPartHpEvents)
                {
                    var bodyPart = bodyPartHpEvent.BodyPart;
                    var hpEvent = bodyPartHpEvent.HpSystemEvent;
                    Debug.Log($"[HP event] {bodyPart.NameCustom}: {hpEvent.HpPrev}->{hpEvent.HpCurrent}HP:");
                }
            }
        }

        protected override void Update(float time, float deltaTime)
        {
            var damageables = EntityManager.GetComponents<DamageableComponent>();

            // placeholder test to apply random damage
            // RandomDamageTest(damageables);

            // process system event queue
            while (_eventSystem.HasEvents)
            {
                var damageableSystemEvent = _eventSystem.DequeueEvent();
                var damageable = damageableSystemEvent.Damageable; // damageable to apply damage to

                // EVENT TYPE 1:
                // process Damage Events
                if (damageableSystemEvent is DamageableSystemDamageEvent damageEvent)
                {
                    var damage = damageEvent.Damage; // damage to apply

                    // apply damage to damageable's Body
                    var bodyDamageEvent = damageable.Body.TakeDamage(damage); // apply damage to damageable
                    OnBodyDamageEvent(bodyDamageEvent, damage);

                    // notify listeners
                    foreach (var listener in damageable.EventListeners)
                        listener.OnEvent(new DamageableComponentEvent(damageable));
                }

                // EVENT TYPE 2:
                // process Capacities Events
                else if (damageableSystemEvent is DamageableSystemCapacitiesEvent capacitiesEvent)
                {
                    damageable.Body.HealingAmountMultiplier = capacitiesEvent.Capacities.capacityInfoCurrent.Healing;
                }
            }

            // main loop over all damageables
            foreach (var damageable in damageables)
            {
                // PART 1: HEALING
                if (damageable.IsDamaged)
                {
                    damageable.Body.TimeSinceHeal += deltaTime;
                    if (damageable.Body.HealingPeriod < damageable.Body.TimeSinceHeal)
                    {
                        damageable.Body.Heal();
                        damageable.Body.TimeSinceHeal = 0;

                        // notify listeners
                        foreach (var listener in damageable.EventListeners)
                            listener.OnEvent(new DamageableComponentEvent(damageable));
                    }

                    // if full health, reset healing timer
                    if (!damageable.IsDamaged)
                        damageable.Body.TimeSinceHeal = 0;
                }

                // PART 2: CHECK CAPACITIES
                if (damageable.CapacitiesDirty)
                {
                    this.OnEvent(new DamageableSystemCapacitiesEvent(damageable));
                    damageable.CapacitiesDirty = false;
                }
            }
        }

        private void RandomDamageTest(List<DamageableComponent> damageables)
        {
            foreach (var damageable in damageables)
            {
                if (UnityEngine.Random.value < 0.005f)
                {
                    // setup damage to apply
                    var amount = 5f;
                    var damage = Damage.PiercingDamage(amount);

                    // setup event
                    var damageableComponentEvent = new DamageableSystemDamageEvent(damageable, damage);

                    // add damage event to system queue
                    this.OnEvent(damageableComponentEvent);
                }
            }
        }
    }
}
