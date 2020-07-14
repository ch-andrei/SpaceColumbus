using System;
using System.Text;
using UnityEngine;

using Common;

namespace Entities.Capacities
{
    // Pain: capacity to feel pain
    // Cognition: capacity to perform orders and to reason in the world
    // Movement: capacity to move, i.e. movement speed
    // Manipulation: capacity to manually perform tasks, i.e. speed of performing tasks
    // Communication: capacity to communicate/cooperate with other agents (vision range, teamwork, etc.)
    // Healing: capacity to recover health
    public enum ECapacityType
    {
        Pain,
        Cognition,
        Movement,
        Manipulation,
        Communication,
        Healing
    }

    public static class CapacityTypes
    {
        public const string PainName = "Pain";
        public const string CognitionName = "Cognition";
        public const string MovementName = "Movement";
        public const string ManipulationName = "Manipulation";
        public const string CommunicationName = "Communication";
        public const string HealingName = "Healing";

        public static string CapacityType2Str(ECapacityType capacityType)
        {
            switch (capacityType)
            {
                case ECapacityType.Pain:
                    return PainName;
                case ECapacityType.Cognition:
                    return CognitionName;
                case ECapacityType.Movement:
                    return MovementName;
                case ECapacityType.Manipulation:
                    return ManipulationName;
                case ECapacityType.Communication:
                    return CommunicationName;
                case ECapacityType.Healing:
                    return HealingName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(capacityType), capacityType, "Unsupported capacity type.");
            }
        }

        public static ECapacityType CapacityStr2Type(string capacityName)
        {
            switch (capacityName)
            {
                case PainName:
                    return ECapacityType.Pain;
                case CognitionName:
                    return ECapacityType.Cognition;
                case MovementName:
                    return ECapacityType.Movement;
                case ManipulationName:
                    return ECapacityType.Manipulation;
                case CommunicationName:
                    return ECapacityType.Communication;
                case HealingName:
                    return ECapacityType.Healing;
                default:
                    throw new ArgumentOutOfRangeException(capacityName, "Unsupported capacity type.");
            }
        }
    }

    public struct CapacityInfo : ICloneable<CapacityInfo>
    {
        public float Pain; // incurred through damaged body parts (missing health)
        public float Cognition; // ability to perform orders and to reason
        public float Movement; // movement speed
        public float Manipulation; // speed of performing tasks
        public float Communication; // vision range, ability to cooperate with other agents
        public float Healing; // vision range, ability to cooperate with other agents

        public CapacityInfo(float pain, float cognition, float movement, float manipulation, float communication, float healing)
        {
            this.Pain = pain;
            this.Cognition = cognition;
            this.Movement = movement;
            this.Manipulation = manipulation;
            this.Communication = communication;
            this.Healing = healing;

            this.Clamp();
        }

        public CapacityInfo(float value) : this(value, value, value, value, value, value) { }

        public CapacityInfo(CapacityInfo capacityInfo) : this(
            capacityInfo.Pain, capacityInfo.Cognition, capacityInfo.Movement,
            capacityInfo.Manipulation, capacityInfo.Communication, capacityInfo.Healing) { }

        public CapacityInfo Clone() => new CapacityInfo(this);

        // clips all values to range 0-1
        public void Clamp()
        {
            this.Pain = Mathf.Max(this.Pain, 0);
            this.Cognition = Mathf.Max(this.Cognition, 0);
            this.Movement = Mathf.Max(this.Movement, 0);
            this.Manipulation = Mathf.Max(this.Manipulation, 0);
            this.Communication = Mathf.Max(this.Communication, 0);
            this.Healing = Mathf.Max(this.Healing, 0);
        }

        public void SetCapacity(ECapacityType capacityType, float value)
        {
            switch (capacityType)
            {
                case ECapacityType.Pain:
                    this.Pain = value;
                    break;
                case ECapacityType.Cognition:
                    this.Cognition = value;
                    break;
                case ECapacityType.Movement:
                    this.Movement = value;
                    break;
                case ECapacityType.Manipulation:
                    this.Manipulation = value;
                    break;
                case ECapacityType.Communication:
                    this.Communication = value;
                    break;
                case ECapacityType.Healing:
                    this.Healing = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(capacityType), capacityType, "Trying to set an unsupported Capacity type.");
            }
        }

        #region operators
        public static CapacityInfo operator -(CapacityInfo lhs, CapacityInfo rhs)
            => new CapacityInfo(
                lhs.Pain - rhs.Pain,
                lhs.Cognition - rhs.Cognition,
                lhs.Movement - rhs.Movement,
                lhs.Manipulation - rhs.Manipulation,
                lhs.Communication - rhs.Communication,
                lhs.Healing - rhs.Healing
            );

        public static CapacityInfo operator +(CapacityInfo lhs, CapacityInfo rhs)
            => new CapacityInfo(
                lhs.Pain + rhs.Pain,
                lhs.Cognition + rhs.Cognition,
                lhs.Movement + rhs.Movement,
                lhs.Manipulation + rhs.Manipulation,
                lhs.Communication + rhs.Communication,
                lhs.Healing + rhs.Healing
            );

        public static CapacityInfo operator *(CapacityInfo lhs, CapacityInfo rhs)
            => new CapacityInfo(
                lhs.Pain * rhs.Pain,
                lhs.Cognition * rhs.Cognition,
                lhs.Movement * rhs.Movement,
                lhs.Manipulation * rhs.Manipulation,
                lhs.Communication * rhs.Communication,
                lhs.Healing * rhs.Healing
            );

        public static CapacityInfo operator +(float rhs, CapacityInfo lhs) => lhs + rhs;
        public static CapacityInfo operator +(CapacityInfo lhs, float rhs)
            => new CapacityInfo(
                lhs.Pain + rhs,
                lhs.Cognition + rhs,
                lhs.Movement + rhs,
                lhs.Manipulation + rhs,
                lhs.Communication + rhs,
                lhs.Healing + rhs
            );

        public static CapacityInfo operator *(float rhs, CapacityInfo lhs) => lhs * rhs;
        public static CapacityInfo operator *(CapacityInfo lhs, float rhs)
            => new CapacityInfo(
                lhs.Pain * rhs,
                lhs.Cognition * rhs,
                lhs.Movement * rhs,
                lhs.Manipulation * rhs,
                lhs.Communication * rhs,
                lhs.Healing * rhs
            );

        #endregion operators

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Capacities: ");
            sb.Append(CapacityTypes.PainName + ": " + this.Pain + "; ");
            sb.Append(CapacityTypes.CognitionName + ": " + this.Cognition + "; ");
            sb.Append(CapacityTypes.MovementName + ": " + this.Movement + "; ");
            sb.Append(CapacityTypes.ManipulationName + ": " + this.Manipulation + "; ");
            sb.Append(CapacityTypes.CommunicationName + ": " + this.Communication + "; ");
            sb.Append(CapacityTypes.HealingName + ": " + this.Healing + ".");

            return sb.ToString();
        }
    }

    public struct CapacityInfoModifier
    {
        public Timed timed;
        public CapacityInfo CapacityInfo;
        public bool Multiplicative;

        public static void Apply(CapacityInfoModifier modifier, ref CapacityInfo capacityInfo)
        {
            if (modifier.Multiplicative)
                capacityInfo *= modifier.CapacityInfo;
            else
                capacityInfo += modifier.CapacityInfo;
        }
    }
}
