using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Utilities.Misc;
using Entities.Bodies.Health;
using Entities.Materials;

namespace Entities.Bodies.Damages
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

    public class Damage
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

        #region StaticDefs
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

        #endregion StaticDefs

        public DamageType DamageType;
        public float Amount;
        public float Dispersion;

        public Damage(DamageType damageType, float amount, float dispersion=1f)
        {
            this.DamageType = damageType;
            this.Amount = amount;
            this.Dispersion = Mathf.Clamp(dispersion, 0, 1);
        }

        public Damage(Damage damage) : this(damage.DamageType, damage.Amount, damage.Dispersion) { }

        public float GetDamageAmountAfterMultiplier(DamageMultiplier damageMultiplier)
        {
            return GetDamageAmountAfterMultiplier(new List<DamageMultiplier>() { damageMultiplier });
        }

        public float GetDamageAmountAfterMultiplier(List<DamageMultiplier> multipliers)
        {
            float damageAmount = this.Amount;
            foreach (var multiplier in multipliers)
                if (this.DamageType == multiplier.DamageType)
                    damageAmount *= multiplier.Amount;
            return damageAmount;
        }

        public Damage SlashingDamage(float amount) { return new Damage(DamageType.Slashing, amount, 0.5f); }
        public Damage PiercingDamage(float amount) { return new Damage(DamageType.Piercing, amount, 0.1f); }
        public Damage BluntDamage(float amount) { return new Damage(DamageType.Blunt, amount, 0.25f); }
        public Damage ChemicalDamage(float amount) { return new Damage(DamageType.Chemical, amount, 0.75f); }
        public Damage ElectricDamage(float amount) { return new Damage(DamageType.Electric, amount, 1f); }
    }

    public class DamageMultiplier : Damage
    {
        // acts as a multiplier to damage
        // less than 1 implies resistance
        // higher than 1 implies weakness
        public DamageMultiplier(DamageType damageType, float amount) : base(damageType, amount) { }
        public DamageMultiplier(DamageMultiplier mult) : base(mult.DamageType, mult.Amount) { }

        #region StaticFunctions

        // if no weights are given, Multipliers get multiplied out
        // if weights are given, Multipliers are averaged given individual weights
        public static List<DamageMultiplier> Simplify(List<DamageMultiplier> multsIn, List<float> weightsIn = null)
        {
            // copy mults
            List<DamageMultiplier> mults = new List<DamageMultiplier>();
            foreach (var mult in multsIn)
                mults.Add(new DamageMultiplier(mult));

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

        public static Damage GetDamageAfterMultiplier(Damage damage, DamageMultiplier multiplier)
        {
            return GetDamageAfterMultiplier(new List<Damage>() { damage }, multiplier)[0];
        }

        public static Damage GetDamageAfterMultiplier(Damage damage, List<DamageMultiplier> multipliers)
        {
            return GetDamageAfterMultiplier(new List<Damage>() { damage }, multipliers)[0];
        }

        public static List<Damage> GetDamageAfterMultiplier(List<Damage> damages, DamageMultiplier multiplier)
        {
            return GetDamageAfterMultiplier(damages, new List<DamageMultiplier>() { multiplier });
        }

        public static List<Damage> GetDamageAfterMultiplier(List<Damage> damages, List<DamageMultiplier> multipliers)
        {
            List<Damage> damagesAfterMultiplier = new List<Damage>();
            foreach (var damage in damages)
            {
                Damage damageAfterMultiplier = new Damage(damage);
                damage.GetDamageAmountAfterMultiplier(multipliers);

                foreach (var multiplier in multipliers)
                    damageAfterMultiplier.Amount = damageAfterMultiplier.GetDamageAmountAfterMultiplier(multiplier);

                damagesAfterMultiplier.Add(damageAfterMultiplier);
            }
            return damagesAfterMultiplier;
        }
        #endregion StaticFunctions
    }
}