using UnityEngine;
using System;
using System.Collections.Generic;

using Utilities.XmlReader;
using Utilities.Misc;

using Common;

using Entities;
using Entities.Health;

namespace Entities.Materials
{
    // TODO: could/should this be a ScriptableObject?
    public class EntityMaterial : INamed
    {
        #region XmlDefs
        private const string MaterialsXmlPath = "Assets/Defs/materials.xml";

        private const string RootField = "root";

        private const string HardnessField = "Hardness";
        private const string RestorationField = "Restoration";
        private const string FlamabilityField = "Flammability";
        private const string DamageMultipliersField = "DamageMultipliers";

        private static XmlReader _materialXmlReader = new XmlReader(MaterialsXmlPath);
        #endregion XmlDefs

        public float Hardness { get; private set; }
        public float Restoration { get; private set; }
        public float Flammability { get; private set; }

        public List<Damage> DamageMultipliers;

        public string Name { get; private set; }

        private EntityMaterial(string name)
        {
            this.Name = name;
            InitializeFromXml();
        }

        private void InitializeFromXml()
        {
            this.Hardness = _materialXmlReader.GetFloat(new List<string>() { RootField, this.Name, HardnessField });
            this.Restoration = _materialXmlReader.GetFloat(new List<string>() { RootField, this.Name, RestorationField });
            this.Flammability = _materialXmlReader.GetFloat(new List<string>() { RootField, this.Name, FlamabilityField });
            InitializeDamageMultipliersFromXml();
        }

        private void InitializeDamageMultipliersFromXml()
        {
            this.DamageMultipliers = new List<Damage>();

            foreach (var damageType in Damages.DamageTypes)
            {
                try
                {
                    // try read damage type multipliers from xml file
                    float multiplier = _materialXmlReader.GetFloat(
                        new List<string>() { RootField, this.Name, DamageMultipliersField, Damages.DamageType2Str(damageType) }
                        );
                    this.DamageMultipliers.Add(new Damage(damageType, multiplier));
                }
                catch (Exception e)
                {
                    this.DamageMultipliers.Add(new Damage(damageType, 1f));
                }
            }
        }

        public static EntityMaterial GetMaterial(string name) { return new EntityMaterial(name); }

        public static EntityMaterial Flesh => new EntityMaterial("Flesh");
        public static EntityMaterial Bone => new EntityMaterial("Bone");
        public static EntityMaterial Steel => new EntityMaterial("Steel");
        public static EntityMaterial Plastic => new EntityMaterial("Plastic");
        public static EntityMaterial Wood => new EntityMaterial("Wood");
        public static EntityMaterial Stone => new EntityMaterial("Stone");
    }
}
