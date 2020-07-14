using System.Collections.Generic;

using UnityEngine;

using Utilities.Events;

namespace Entities.Damageables
{
    public class HpSystemDamageEvent : DamageEvent
    {
        public HpSystemEvent HpSystemEvent;

        public HpSystemDamageEvent(HpSystemEvent hpSystemEvent, List<Damage> damages) : base(damages)
        {
            this.HpSystemEvent = hpSystemEvent;
        }

        public HpSystemDamageEvent(HpSystemEvent hpSystemEvent, Damage damage) :
            this(hpSystemEvent, new List<Damage>() {damage})
        {
        }
    }

    public class HpSystemEvent : GameEvent
    {
        public readonly float HealthDelta;
        public readonly int HpPrev;
        public readonly int HpCurrent;

        public HpSystemEvent(int hpPrev, int hpCurrent, float healthDelta)
        {
            this.HpPrev = hpPrev;
            this.HpCurrent = hpCurrent;
            this.HealthDelta = healthDelta;
        }
    }

    public class HpSystem : ICanBeDamaged, ICanTakeDamage<HpSystemDamageEvent>
    {
        public bool CanBeDamaged => true;
        public bool IsDamaged => this.Health < 1f;

        public int HpBase { get; private set; } // maximum HP
        public float Health { get; private set; } // current health, unit normalized (0-1)

        public int HpCurrent => Mathf.RoundToInt(Health * HpBase);
        public int HpPrev { get; private set; }

        public string AsText => $"HP: [{HpCurrent}/{HpBase}]";

        private List<Damage> _damageMultipliers;

        public HpSystem(int hpBase)
        {
            this.Health = 1f;
            this.HpBase = hpBase;
            this.HpPrev = hpBase;
            this._damageMultipliers = new List<Damage>();
        }

        public HpSystem(int hpBase, List<Damage> damageMultipliers) : this(hpBase)
        {
            foreach (var mult in damageMultipliers)
                this._damageMultipliers.Add(mult.Clone()); // make a new local copy
        }

        public HpSystem(HpSystem hpSystem) : this(hpSystem.HpBase, hpSystem._damageMultipliers)
        {
            this.HpPrev = hpSystem.HpPrev;
            this.Health = hpSystem.Health;
        }

        private HpSystemEvent OnHealthChangeEvent(float healthDelta)
        {
            this.HpPrev = HpCurrent;
            this.Health += healthDelta;
            this.Health = Mathf.Clamp(this.Health, 0, 1);

            return new HpSystemEvent(this.HpPrev, HpCurrent, healthDelta);
        }

        public HpSystemDamageEvent TakeDamage(Damage damage)
        {
            var damageAfterModifier = DamageMultipliers.GetDamageAfterMultiplier(damage, _damageMultipliers);
            float healthDelta = damageAfterModifier.Amount / HpBase; // normalized 0-HpBase -> 0-1 range (could exceed if high dmg)

            var hpSystemEvent = OnHealthChangeEvent(-healthDelta);

            return new HpSystemDamageEvent(hpSystemEvent, damage);
        }

        public HpSystemEvent HealHp(int hp)
        {
            // add a small constant to make sure at least this many HP is healed
            float healthDelta = (hp + 1e-6f) / (float)HpBase;

            return Heal(healthDelta);
        }

        public HpSystemEvent Heal(float healthDelta)
        {
            return OnHealthChangeEvent(healthDelta);
        }

        public EDamageState GetDamageState()
        {
            return DamageStates.HealthToDamageState(this.Health);
        }
    }
}
