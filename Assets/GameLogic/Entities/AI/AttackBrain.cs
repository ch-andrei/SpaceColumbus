using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities.Bodies;

namespace Brains.Attack
{
    public class AttackVariant
    {
        public enum AttackType
        {
            Melee,
            Ranged
        }

        public AttackType Type;
    }

    [System.Serializable]
    public class AttackBrain
    {
        //public static TargetSystem targetSystem;

        public AttackVariant attackVariant;

        public AttackBrain()
        {

        }

        public float GetAttackRange()
        {
            return 0;
        }

        public GameObject GetAttackTarget()
        {
            return null;
        }
    }
}
