using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using Utilities.Misc;

using Common;

using Entities;
using Entities.Damageables;
using UnityEngine.AI;
using XmlReader = Utilities.XmlReader.XmlReader;

namespace Entities.Materials
{
    public static class EntityMaterialFactory
    {
        #region XmlDefs
        private const string MaterialsXmlPath = "Assets/Defs/materials.xml";

        private const string HardnessField = "Hardness";
        private const string RestorationField = "Restoration";
        private const string FlammabilityField = "Flammability";
        private const string DamageMultipliersField = "DamageMultipliers";

        private const string FleshMaterialName = "Flesh";
        private const string BoneMaterialName = "Bone";
        private const string SteelMaterialName = "Steel";
        private const string PlasticMaterialName = "Plastic";
        private const string WoodMaterialName = "Wood";
        private const string StoneMaterialName = "Stone";

        private static XmlReader _materialXmlReader = new XmlReader(MaterialsXmlPath);
        #endregion XmlDefs

        public static Dictionary<string, EntityMaterial> Materials;

        public static void Initialize()
        {
            Materials = new Dictionary<string, EntityMaterial>();

            // create and cache materials
            CreateMaterial(FleshMaterialName);
            CreateMaterial(BoneMaterialName);
            CreateMaterial(SteelMaterialName);
            CreateMaterial(PlasticMaterialName);
            CreateMaterial(WoodMaterialName);
            CreateMaterial(StoneMaterialName);
        }

        public static EntityMaterial Flesh => GetMaterial(FleshMaterialName);
        public static EntityMaterial Bone => GetMaterial(BoneMaterialName);
        public static EntityMaterial Steel => GetMaterial(SteelMaterialName);
        public static EntityMaterial Plastic => GetMaterial(PlasticMaterialName);
        public static EntityMaterial Wood => GetMaterial(WoodMaterialName);
        public static EntityMaterial Stone => GetMaterial(StoneMaterialName);

        public static void CreateMaterial(string name)
        {
            var material = new EntityMaterial();
            material.Name = name;

            try
            {
                InitializeFromXml(ref material, name); // create
                Materials[name] = material; // store
            }
            catch (XmlException e)
            {
                Debug.Log($"Warning: MaterialFactory could not create material [{name}].");
            }
        }

        public static EntityMaterial GetMaterial(string name)
        {
            if (Materials.ContainsKey(name))
                return Materials[name].Clone();
            else
            {
                // need to create material first
                CreateMaterial(name);
                return GetMaterial(name);
            }
        }

        private static void InitializeFromXml(ref EntityMaterial material, string name)
        {
            material.Hardness = _materialXmlReader.GetFloat(new List<string>() { name, HardnessField });
            material.Restoration = _materialXmlReader.GetFloat(new List<string>() { name, RestorationField });
            material.Flammability = _materialXmlReader.GetFloat(new List<string>() { name, FlammabilityField });

            var damageTypes = Damages.DamageTypes;
            material.DamageMultipliers = new Damage[damageTypes.Count];
            for (int i = 0; i < damageTypes.Count; i++)
            {
                var damageType = damageTypes[i];

                // not all types of damage may be present in the xml defs; default fallback is unit (1.0) multiplier
                float multiplier = 1f;
                try
                {
                    // try read damage type multipliers from xml file
                    multiplier = _materialXmlReader.GetFloat(
                        new List<string>() { name, DamageMultipliersField, Damages.DamageType2Str(damageType) }
                    );
                }
                catch (Exception e) { }

                material.DamageMultipliers[i] = new Damage(damageType, multiplier);
            }
        }
    }

    public struct EntityMaterial : INamed, ICloneable<EntityMaterial>
    {
        public float Hardness;
        public float Restoration;
        public float Flammability;

        public Damage[] DamageMultipliers;

        public string Name { get; set; }

        public EntityMaterial(float hardness, float restoration, float flammability, string name = "")
        {
            this.Hardness = hardness;
            this.Restoration = restoration;
            this.Flammability = flammability;
            this.DamageMultipliers = new Damage[Damages.DamageTypes.Count];
            this.Name = name;
        }

        public EntityMaterial(EntityMaterial material) : this(
            material.Hardness,
            material.Restoration,
            material.Flammability,
            string.Copy(material.Name)
            )
        {
            for (int i = 0; i < material.DamageMultipliers.Length; i++)
                this.DamageMultipliers[i] = material.DamageMultipliers[i].Clone();
        }

        public EntityMaterial Clone() => new EntityMaterial(this);
    }
}
