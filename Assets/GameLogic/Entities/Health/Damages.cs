using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Common;
using Entities.Materials;

using Utilities.Misc;

namespace Entities.Health
{
    public enum EDamageType : byte
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

    public struct Damage : ICloneable<Damage>
    {
        public EDamageType DamageType;
        public float Amount;
        public float Penetration;
        public float Dispersion;

        public Damage(EDamageType damageType, float amount, float penetration=1f, float dispersion=1f)
        {
            this.DamageType = damageType;
            this.Amount = amount;
            this.Penetration = Mathf.Clamp(penetration, 0, 1);
            this.Dispersion = Mathf.Clamp(dispersion, 0, 1);
        }

        public Damage(float amount, float penetration = 1f, float dispersion = 1f) :
            this(EDamageType.None, amount, penetration, dispersion) { }

        public Damage(Damage damage) : this(damage.DamageType, damage.Amount, damage.Penetration, damage.Dispersion) { }

        public static Damage operator *(Damage lhs, float mult)
        {
            var dmg = new Damage(lhs);
            dmg.Amount *= mult;
            return lhs;
        }

        public static Damage SlashingDamage(float amount)
        {
            return new Damage(EDamageType.Slashing, amount, 0.2f, 0.3f); // medium penetration, medium dispersion
        }

        public static Damage PiercingDamage(float amount)
        {
            return new Damage(EDamageType.Piercing, amount, 0.5f, 0.05f); // high penetration, low dispersion
        }

        public static Damage BluntDamage(float amount)
        {
            return new Damage(EDamageType.Blunt, amount, 0.05f, 0.25f); // very low penetration, medium dispersion
        }

        public static Damage ChemicalDamage(float amount)
        {
            return new Damage(EDamageType.Chemical, amount, 0.05f, 1f); // very low penetration, very high dispersion
        }

        public static Damage ElectricDamage(float amount)
        {
            return new Damage(EDamageType.Electric, amount, 1f, 1f); // very high penetration, very high dispersion
        }

        public Damage Clone() => new Damage(this);
    }

    public static class Damages
    {
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

        private static List<EDamageType> _damageTypes = null;
        public static List<EDamageType> DamageTypes
        {
            get
            {
                // this will be built on first call
                if (_damageTypes is null)
                {
                    _damageTypes = new List<EDamageType>();
                    foreach (EDamageType damageType in EDamageType.GetValues(typeof(EDamageType)))
                    {
                        if (damageType != EDamageType.None)
                            _damageTypes.Add(damageType);
                    }
                }

                return _damageTypes;
            }
        }

        public static string ToString(this EDamageType type) => DamageType2Str(type);

        public static string DamageType2Str(EDamageType eDamageType)
        {
            switch (eDamageType)
            {
                case EDamageType.Blunt:
                    return DamageBluntName;
                case EDamageType.Slashing:
                    return DamageSlashingName;
                case EDamageType.Piercing:
                    return DamagePiercingName;
                case EDamageType.Heat:
                    return DamageHeatName;
                case EDamageType.Electric:
                    return DamageElectricName;
                case EDamageType.Chemical:
                    return DamageChemicalName;
                case EDamageType.Psychological:
                    return DamagePsychologicalName;
                case EDamageType.Emp:
                    return DamageEmpName;
                default:
                    return DamageNoneName;
            }
        }

        public static EDamageType DamageStr2Type(string damageType)
        {
            switch (damageType)
            {
                case DamageBluntName:
                    return EDamageType.Blunt;
                case DamageSlashingName:
                    return EDamageType.Slashing;
                case DamagePiercingName:
                    return EDamageType.Piercing;
                case DamageHeatName:
                    return EDamageType.Heat;
                case DamageElectricName:
                    return EDamageType.Electric;
                case DamageChemicalName:
                    return EDamageType.Chemical;
                case DamagePsychologicalName:
                    return EDamageType.Psychological;
                case DamageEmpName:
                    return EDamageType.Emp;
                default:
                    return EDamageType.None;
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
        public static Damage Multiplier(EDamageType eDamageType, float amount)
        {
            return new Damage(eDamageType, amount);
        }

        public static Damage Multiplier(float amount)
        {
            return Multiplier(EDamageType.None, amount);
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
                for (int j = i + 1; j < count - simplified; j++)
                {
                    var m2 = mults[j];

                    if (m1.DamageType == m2.DamageType)
                    {
                        // simplify same type multipliers
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
                if (damage.DamageType == multiplier.DamageType || multiplier.DamageType == EDamageType.None)
                    damageAmount *= multiplier.Amount;
            return damageAmount;
        }
    }
}
