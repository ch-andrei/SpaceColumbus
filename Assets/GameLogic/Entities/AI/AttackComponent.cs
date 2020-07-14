using System;
using UnityEngine;

namespace Brains.Attack
{
    public interface IAttackComponent
    {
        bool CanAttack { get; }
        float GetAttackRange();
        GameObject GetAttackTarget(out Vector3 target);
    }

    public class AttackVariant
    {
        public enum AttackType
        {
            Melee, // contact-based short-range 'ranged' attack
            Ranged // contact-less ranged attack
        }

        public AttackType Type;
    }

    [System.Serializable]
    public class AttackComponent : IAttackComponent
    {
        //public static TargetSystem targetSystem;
        public bool CanAttack => false;

        public AttackVariant attackVariant;

        public AttackComponent()
        {

        }

        public float GetAttackRange()
        {
            return 0;
        }

        public GameObject GetAttackTarget(out Vector3 target)
        {
            throw new NotImplementedException();
        }
    }
}
