using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities.Events;

using Entities.Materials;
using Entities.Bodies;

using UI.Utils;

namespace Entities.Damageables
{
    public interface ICanBeDamaged
    {
        bool CanBeDamaged { get; } // true if this object can be applied damage to
        bool IsDamaged { get; } // true if this object is at full health
        EDamageState GetDamageState(); // current damage state
    }

    public interface ICanTakeDamage<T> where T : DamageEvent
    {
        // implementations must ensure that damage be applied immediately during the call to TakeDamage().
        // a completed damage event must be immediately generated (and not scheduled to be processed later)
        T TakeDamage(Damage damage);
    }

    // public interface ICanTakeDamage : ICanTakeDamage<DamageEvent>
    // {
    // }

    public enum EDamageState : byte
    {
        None = 0,
        Minor = 1,
        Major = 2,
        Critical = 3,
        Terminal = 4
    }

    public static class DamageStates
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
