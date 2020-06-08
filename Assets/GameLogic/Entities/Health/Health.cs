using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities.Events;

using Entities.Materials;
using Entities.Bodies;

using UI.Utils;

namespace Entities.Health
{
    public class HpSystemEvent : DamageableEvent
    {
        public readonly HpSystem HpSystem;
        public readonly float HealthDelta;

        public HpSystemEvent(List<Damage> damages, HpSystem hpSystem, float healthDelta) : base(damages, null)
        {
            this.HpSystem = hpSystem;
            this.HealthDelta = healthDelta;
        }
    }

    public class HpSystem : EventGenerator<HpSystemEvent>, IDamageable
    {
        public bool IsDamageable => true;
        public bool IsDamaged => this.Health < 1f;

        public int HpBase { get; private set; } // maximum HP

        public float Health { get; private set; } // always between 0 and 1

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
                this._damageMultipliers.Add(new Damage(mult)); // make a new local copy
        }

        public HpSystem(int hpBase, Damage damageMultiplier) :
            this(hpBase, new List<Damage>(){ damageMultiplier }) { }

        public HpSystem(HpSystem hpSystem) : this(hpSystem.HpBase, hpSystem._damageMultipliers)
        {
            this.HpPrev = hpSystem.HpPrev;
            this.Health = hpSystem.Health;

            // copy listeners
            foreach (var listener in hpSystem.EventListeners)
                EventListeners.Add(listener);
        }

        private void OnHealthChangeEvent(float healthDelta, List<Damage> damages)
        {
            this.HpPrev = HpCurrent;

            this.Health -= healthDelta;
            this.Health = Mathf.Clamp(this.Health, 0, 1);

            Notify(new HpSystemEvent(damages, this, healthDelta));
        }

        public void TakeDamage(Damage damage)
        {
            TakeDamage(new List<Damage>() { damage });
        }

        public void TakeDamage(List<Damage> damages)
        {
            List<Damage> damagesAfterModifier = DamageMultipliers.GetDamageAfterMultiplier(damages, _damageMultipliers);
            float totalDamage = Damages.GetTotalDamage(damagesAfterModifier);
            float healthDelta = totalDamage / HpBase;

            OnHealthChangeEvent(healthDelta, damagesAfterModifier);
        }

        public void Heal(float healthDelta)
        {
            OnHealthChangeEvent(healthDelta, new List<Damage>());
        }

        public void HealHp(int hp)
        {
            // add a small constant to make sure at least this many HP is healed
            // meanwhile, this also ensures float division
            float healthDelta = (hp + 1e-6f) / HpBase;

            OnHealthChangeEvent(healthDelta, new List<Damage>());
        }

        public EDamageState GetDamageState()
        {
            return HpSystemDamageStates.HealthToDamageState(this.Health);
        }
    }

    public static class HpSystemDamageStates
    {
        #region XmlDefs

        // names
        public const string NoneDamageStateName = "None";
        public const string MinorDamageStateName = "Minor";
        public const string MajorDamageStateName = "Major";
        public const string CriticalDamageStateName = "Critical";
        public const string TerminalDamageStateName = "Terminal";

        // thresholds are for equal or less than
        public const float MinorDamageStateThreshold = 0.99f;
        public const float MajorDamageStateThreshold = 0.7f;
        public const float CriticalDamageStateThreshold = 0.25f;
        public const float TerminalDamageStateThreshold = 0.05f;

        #endregion XmlDefs

        #region UI
        // TODO: refactor this region out of game logic

        // colors
        public static Color NoneDamageStateColor = new Color(0, 1, 0);
        public static Color MinorDamageStateColor = new Color(0.5f, 1, 0);
        public static Color MajorDamageStateColor = new Color(1, 1, 0);
        public static Color CriticalDamageStateColor = new Color(1, 0.5f, 0);
        public static Color TerminalDamageStateColor = new Color(1, 0, 0);

        public static Color DamageStateToColor(EDamageState damageState)
        {
            switch (damageState)
            {
                case EDamageState.Terminal:
                    return TerminalDamageStateColor;
                case EDamageState.Critical:
                    return CriticalDamageStateColor;
                case EDamageState.Major:
                    return MajorDamageStateColor;
                case EDamageState.Minor:
                    return MinorDamageStateColor;
                default:
                    return NoneDamageStateColor;
            }
        }

        public static string DamageStateToStr(EDamageState damageState)
        {
            switch (damageState)
            {
                case EDamageState.Terminal:
                    return TerminalDamageStateName;
                case EDamageState.Critical:
                    return CriticalDamageStateName;
                case EDamageState.Major:
                    return MajorDamageStateName;
                case EDamageState.Minor:
                    return MinorDamageStateName;
                default:
                    return NoneDamageStateName;
            }
        }

        public static string DamageStateToStrWithColor(EDamageState damageState)
        {
            return RichStrings.WithColor(DamageStateToStr(damageState), DamageStateToColor(damageState));
        }
        #endregion UI

        public static EDamageState GetWorstDamageState(EDamageState s1, EDamageState s2)
        {
            return (EDamageState)Mathf.Max((int)s1, (int)s2);
        }

        public static EDamageState HealthToDamageState(float health)
        {
            health = Mathf.Clamp(health, 0, 1);
            if (health <= TerminalDamageStateThreshold)
                return EDamageState.Terminal;
            else if (health <= CriticalDamageStateThreshold)
                return EDamageState.Critical;
            else if (health <= MajorDamageStateThreshold)
                return EDamageState.Major;
            else if (health <= MinorDamageStateThreshold)
                return EDamageState.Minor;
            else
                return EDamageState.None;
        }
    }
}
