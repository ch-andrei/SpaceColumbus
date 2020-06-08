using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Entities.Materials;

using Utilities.Misc;

namespace Entities.Health
{
    public enum DamageType : byte
    {
        None,
        Blunt,
        Slashing,
        Piercing,
        Heat,
        Electric,
        Chemical,
        Psychological,
        Emp,
    }

    public struct Damage
    {
        public DamageType DamageType;
        public float Amount;

        // proportion of damage carried over to the next level of pdeth
        public float Penetration;

        /*
         * Dispersion = sharing ratio of damage across the damaged components.
         * Example:
         *     Attempting to damage N objects with dispersion=0 damage will only apply damage to 1 of the
         *         components picked at random.
         *     Attempting to damage N objects with dispersion=1 damage will damage all of the components.
         *     Sharing across damaged components should be
        */
        public float Dispersion;

        public Damage(DamageType damageType, float amount, float penetration=1f, float dispersion=1f)
        {
            this.DamageType = damageType;
            this.Amount = amount;
            this.Penetration = Mathf.Clamp(penetration, 0, 1);
            this.Dispersion = Mathf.Clamp(dispersion, 0, 1);
        }

        public Damage(float amount, float penetration = 1f, float dispersion = 1f) :
            this(DamageType.None, amount, penetration, dispersion) { }

        public Damage(Damage damage) : this(damage.DamageType, damage.Amount, damage.Dispersion) { }
    }

    public static class Damages
    {

        public static Damage SlashingDamage(float amount)
        {
            return new Damage(DamageType.Slashing, amount, 0.25f, 0.75f); // low penetration, high dispersion
        }

        public static Damage PiercingDamage(float amount)
        {
            return new Damage(DamageType.Piercing, amount, 0.75f, 0.1f); // high penetration, low dispersion
        }

        public static Damage BluntDamage(float amount)
        {
            return new Damage(DamageType.Blunt, amount, 0.1f, 0.5f); // very low penetration, medium dispersion
        }

        public static Damage ChemicalDamage(float amount)
        {
            return new Damage(DamageType.Chemical, amount, 0.05f, 1f); // very low penetration, high dispersion
        }

        public static Damage ElectricDamage(float amount)
        {
            return new Damage(DamageType.Electric, amount, 1f, 1f); // very high penetration, very high dispersion
        }

        #region XmlDefs
        public const string DamageNoneName = "None";
        public const string DamageBluntName = "Blunt";
        public const string DamageSlashingName = "Slashing";
        public const string DamagePiercingName = "Piercing";
        public const string DamageHeatName = "Heat";
        public const string DamageElectricName = "Electric";
        public const string DamageChemicalName = "Chemical";
        public const string DamagePsychologicalName = "Psychological";
        public const string DamageEmpName = "EMP";
        #endregion XmlDefs

        private static List<DamageType> _damageTypes = null;
        public static List<DamageType> DamageTypes
        {
            get
            {
                // this will be built on first call
                if (_damageTypes is null)
                {
                    _damageTypes = new List<DamageType>();
                    foreach (DamageType damageType in DamageType.GetValues(typeof(DamageType)))
                    {
                        if (damageType != DamageType.None)
                            _damageTypes.Add(damageType);
                    }
                }

                return _damageTypes;
            }
        }

        public static string DamageType2Str(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Blunt:
                    return DamageBluntName;
                case DamageType.Slashing:
                    return DamageSlashingName;
                case DamageType.Piercing:
                    return DamagePiercingName;
                case DamageType.Heat:
                    return DamageHeatName;
                case DamageType.Electric:
                    return DamageElectricName;
                case DamageType.Chemical:
                    return DamageChemicalName;
                case DamageType.Psychological:
                    return DamagePsychologicalName;
                case DamageType.Emp:
                    return DamageEmpName;
                default:
                    return DamageNoneName;
            }
        }

        public static DamageType DamageStr2Type(string damageType)
        {
            switch (damageType)
            {
                case DamageBluntName:
                    return DamageType.Blunt;
                case DamageSlashingName:
                    return DamageType.Slashing;
                case DamagePiercingName:
                    return DamageType.Piercing;
                case DamageHeatName:
                    return DamageType.Heat;
                case DamageElectricName:
                    return DamageType.Electric;
                case DamageChemicalName:
                    return DamageType.Chemical;
                case DamagePsychologicalName:
                    return DamageType.Psychological;
                case DamageEmpName:
                    return DamageType.Emp;
                default:
                    return DamageType.None;
            }
        }

        public static float GetTotalDamage(List<Damage> damages)
        {
            float totalDamage = 0f;
            foreach (var damage in damages)
                totalDamage += damage.Amount;
            return totalDamage;
        }
    }

    public static class DamageMultipliers
    {
        public static Damage Multiplier(DamageType damageType, float amount)
        {
            return new Damage(damageType, amount);
        }

        public static Damage Multiplier(float amount)
        {
            return Multiplier(DamageType.None, amount);
        }

        // use Damage as a multiplier to other damages
        // amount less than 1 implies resistance
        // amount higher than 1 implies weakness

        // if no weights are given, Multipliers get multiplied out
        // if weights are given, Multipliers are averaged given individual weights
        public static List<Damage> Simplify(List<Damage> multsIn, List<float> weightsIn = null)
        {
            // copy mults
            List<Damage> mults = new List<Damage>();
            foreach (var mult in multsIn)
                mults.Add(new Damage(mult));

            // copy weights
            List<float> weights = new List<float>();
            if (weightsIn != null)
                weights.AddRange(weightsIn);

            int count = mults.Count;
            int simplified = 0;
            for (int i = 0; i < count - simplified; i++)
            {
                var m1 = mults[i];
                for (int j = 0; j < count - simplified; j++)
                {
                    if (i == j)
                        continue;

                    var m2 = mults[j];

                    if (m1.DamageType == m2.DamageType)
                    {
                        if (weightsIn == null)
                            m1.Amount *= m2.Amount;
                        else
                        {
                            m1.Amount = m1.Amount * weights[i] + m2.Amount * weights[j];
                            weights[i] += weights[j];
                            m1.Amount /= weights[i];
                            weights.RemoveAt(j);
                        }
                        mults.RemoveAt(j);
                        simplified++;
                    }
                }
            }

            return mults;
        }

        public static Damage GetDamageAfterMultiplier(Damage damage, Damage multiplier)
        {
            return GetDamageAfterMultiplier(new List<Damage>() { damage }, multiplier)[0];
        }

        public static Damage GetDamageAfterMultiplier(Damage damage, List<Damage> multipliers)
        {
            return GetDamageAfterMultiplier(new List<Damage>() { damage }, multipliers)[0];
        }

        public static List<Damage> GetDamageAfterMultiplier(List<Damage> damages, Damage multiplier)
        {
            return GetDamageAfterMultiplier(damages, new List<Damage>() { multiplier });
        }

        public static List<Damage> GetDamageAfterMultiplier(List<Damage> damages, List<Damage> multipliers)
        {
            var damagesAfterMultiplier = new List<Damage>();
            foreach (var damage in damages)
            {
                // struct for holding multiplied damage to not overwrite original damage struct
                var damageAfterMultiplier = new Damage(damage);

                // apply each damage multiplier
                foreach (var multiplier in multipliers)
                    damageAfterMultiplier.Amount =
                        GetDamageAmountAfterMultiplier(damageAfterMultiplier, multiplier);

                damagesAfterMultiplier.Add(damageAfterMultiplier);
            }
            return damagesAfterMultiplier;
        }

        public static float GetDamageAmountAfterMultiplier(Damage damage, Damage multiplier)
        {
            return GetDamageAmountAfterMultiplier(damage, new List<Damage>() { multiplier });
        }

        public static float GetDamageAmountAfterMultiplier(Damage damage, List<Damage> multipliers)
        {
            float damageAmount = damage.Amount;
            foreach (var multiplier in multipliers)
                if (damage.DamageType == multiplier.DamageType || multiplier.DamageType == DamageType.None)
                    damageAmount *= multiplier.Amount;
            return damageAmount;
        }
    }
}
