using System;
using System.Collections.Generic;
using System.Xml;
using Entities.Capacities;
using Utilities.Misc;
using Entities.Damageables;
using Entities.Materials;
using UnityEngine;
using XmlReader = Utilities.XmlReader.XmlReader;

namespace Entities.Bodies
{
    public static class BodyFactory
    {
        #region XmlDefs

        private const string BodiesXmlPath = "Assets/Defs/bodyparts.xml";

        private const string BodyPartsField = "BodyParts";
        private const string BodyStatsField = "BodyStats";
        private const string CapacityStatsField = "CapacityStats";
        private const string InclusionField = "Inclusion";

        private const string CriticalField = "Critical";

        private const string HpField = "hp";
        private const string SizeField = "size";
        private const string MaterialsField = "mat";

        private const string DefaultField = "Default";
        private const string HealingPeriodField = "HealingSpeed";
        private const string HealingAmountField = "HealingAmount";

        private static XmlReader _bodyPartXmlReader = new XmlReader(BodiesXmlPath);
        #endregion XmlDefs

        #region Bodies Generation
        public const string HumanoidBodyName = "Humanoid";
        public const string TorsoName = "Torso";
        public const string HeadName = "Head";

        public static Body HumanoidBody => GetBody(HumanoidBodyName);
        public static BodyPart HumanoidTorso => GetBodyPart(HumanoidBodyName, TorsoName);
        public static BodyPart HumanoidHead => GetBodyPart(HumanoidBodyName, HeadName);

        #endregion Bodies Generation

        public static Dictionary<string, Body> AvailableBodies = new Dictionary<string, Body>();
        public static Dictionary<string, Dictionary<string, BodyPart>> AvailableBodyParts = new Dictionary<string, Dictionary<string, BodyPart>>();
        public static bool IsInitialized = false;

        public static void Initialize()
        {
            ReadBodyPartsFromXml();
        }

        public static Body GetBody(EBodyType bodyType) => GetBody(BodyTypes.BodyType2String(bodyType));
        public static Body GetBody(string bodyType)
        {
            var body = AvailableBodies[bodyType];
            return body.Clone();
        }

        public static BodyPart GetBodyPart(EBodyType bodyType, string name) => GetBodyPart(BodyTypes.BodyType2String(bodyType), name);
        public static BodyPart GetBodyPart(string bodyType, string name)
        {
            try
            {
                var bodyPart = AvailableBodyParts[bodyType][name];
                return bodyPart.Clone();
            }
            catch (KeyNotFoundException e)
            {
                LoggerDebug.LogE($"Could not find body part: {bodyType}/{name}");
                throw e;
            }
        }

        public static void ReadBodyPartsFromXml()
        {
            IsInitialized = true;

            AvailableBodyParts = new Dictionary<string, Dictionary<string, BodyPart>>();

            // STEP 1:
            // read names only
            var bodyVariantsNames = _bodyPartXmlReader.GetChildren(new List<string>() { BodyPartsField });
            var bodyPartNamesPerVariant = new List<List<string>>();
            int countVariants = 0;
            foreach (var variantName in bodyVariantsNames)
            {
                bodyPartNamesPerVariant.Add(new List<string>());

                var bodyPartNames = _bodyPartXmlReader.GetChildren(new List<string>() { BodyPartsField, variantName });
                foreach (var bodyPartName in bodyPartNames)
                {
                    //Console.WriteLine("Adding bodyPartName " + bodyPartName);
                    bodyPartNamesPerVariant[countVariants].Add(bodyPartName);
                }

                countVariants++;
            }

            // STEP 2:
            // setup BodyPart Dictionary
            for (int i = 0; i < bodyVariantsNames.Count; i++)
            {
                var variantName = bodyVariantsNames[i];
                var variantBodyType = BodyTypes.String2BodyType(variantName);

                // check if variant exists
                if (!AvailableBodyParts.ContainsKey(variantName))
                    AvailableBodyParts[variantName] = new Dictionary<string, BodyPart>();

                //Console.WriteLine("variantName: " + variantName);

                foreach (var bodyPartName in bodyPartNamesPerVariant[i])
                {
                    // get base statistics
                    float hp = _bodyPartXmlReader.GetFloat(new List<string>() { BodyPartsField, variantName, bodyPartName, HpField });
                    float size = _bodyPartXmlReader.GetFloat(new List<string>() { BodyPartsField, variantName, bodyPartName, SizeField });

                    // get materials
                    var materialNames = _bodyPartXmlReader.GetChildren(
                        new List<string>() { BodyPartsField, variantName, bodyPartName, MaterialsField });

                    var multipliers = new List<Damage>();
                    var weights = new List<float>();
                    foreach (var materialName in materialNames)
                    {
                        float materialWeight = _bodyPartXmlReader.GetFloat(
                            new List<string>() { BodyPartsField, variantName, bodyPartName, MaterialsField, materialName });
                        var material = EntityMaterialFactory.GetMaterial(materialName);
                        try
                        {
                            foreach (var mult in material.DamageMultipliers)
                            {
                                multipliers.Add(mult);
                                weights.Add(materialWeight);
                            }
                        }
                        catch (Exception e) { /* do nothing */ }
                    }

                    // Debug.Log("Creating bodypart: " + bodyPartName);

                    // Debug.Log("before simplifying:");
                    // foreach (var mult in multipliers)
                    //     Debug.Log($"Multiplier [{mult.DamageType.ToString()}] " + mult.Amount);

                    multipliers = DamageMultipliers.Simplify(multipliers, weights);

                    // Debug.Log("after simplifying:");
                    // foreach (var mult in multipliers)
                    //     Debug.Log($"Multiplier [{mult.DamageType.ToString()}] " + mult.Amount);

                    // read capacities stats from XML
                    List<string> capacitiesNames;
                    try
                    {
                        capacitiesNames = _bodyPartXmlReader.GetChildren(
                            new List<string>() { CapacityStatsField, variantName, bodyPartName });
                    }
                    catch (Exception e)
                    {
                        capacitiesNames = new List<string>();
                    }

                    // build capacities
                    var capacities = new Capacities.CapacityInfo();
                    foreach (var capacityName in capacitiesNames)
                    {
                        try
                        {
                            var value = _bodyPartXmlReader.GetFloat(
                                new List<string>() { CapacityStatsField, variantName, bodyPartName, capacityName });

                            if (capacityName == CriticalField)
                            {
                                Debug.Log($"Got critical value = {value}");
                            }
                            else
                            {
                                var capacityType = CapacityTypes.CapacityStr2Type(capacityName);

                                capacities.SetCapacity(capacityType, value);
                                Debug.Log($"Got capacity {capacityName} = {value}");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"Could not real XML for field {capacityName}");
                        }
                    }

                    // finally, build bodypart
                    var hpSystem = new HpSystem((int)hp, multipliers);
                    var bodyPart = new BodyPart(variantBodyType, hpSystem, bodyPartName, size, capacities);

                    Debug.Log("Created bodypart: " + bodyPart.NameCustom);

                    AvailableBodyParts[variantName][bodyPartName] = bodyPart;
                }
            }

            float defaulthHealingPeriod = _bodyPartXmlReader.GetFloat(
                new List<string>() { BodyStatsField, DefaultField, HealingPeriodField });
            float defaultHealingAmount = _bodyPartXmlReader.GetFloat(
                new List<string>() { BodyStatsField, DefaultField, HealingAmountField });

            // TODO: convert this to recursive function to be able to support deeper body structures
            // STEP 3:
            // build bodies and add parts for containers
            var bodyTypeNames = _bodyPartXmlReader.GetChildren(new List<string>() { InclusionField });
            foreach (var bodyTypeName in bodyTypeNames)
            {
                var eBodyType = BodyTypes.String2BodyType(bodyTypeName);
                var body = new Body(eBodyType);

                var healingPeriod = _bodyPartXmlReader.TryGetFloat(
                    new List<string>() { BodyStatsField, bodyTypeName, HealingPeriodField },
                    defaulthHealingPeriod);
                var healingAmount = _bodyPartXmlReader.TryGetFloat(
                    new List<string>() { BodyStatsField, bodyTypeName, HealingAmountField },
                    defaultHealingAmount);

                body.HealingPeriod = healingPeriod;
                body.HealingAmount = healingAmount;

                var containerNames = _bodyPartXmlReader.GetChildren(new List<string>() { InclusionField, bodyTypeName });

                foreach (var bodyPartName in containerNames)
                {
                    var bodyPart = AvailableBodyParts[bodyTypeName][bodyPartName];
                    var bodyNode = body.AddBodyPart(bodyPart);

                    var partsList = _bodyPartXmlReader.GetChildren(
                        new List<string>() { InclusionField, bodyTypeName, bodyPartName });

                    foreach (var partName in partsList)
                    {
                        var customNames = _bodyPartXmlReader.GetStrings(
                            new List<string>() { InclusionField, bodyTypeName, bodyPartName, partName, XmlReader.ItemField });

                        var bodyPartCustomName = AvailableBodyParts[bodyTypeName][partName].Clone();

                        if (customNames.Count == 0)
                            customNames.Add(bodyPartCustomName.Name);

                        foreach (var customName in customNames)
                        {
                            bodyPartCustomName.NameCustom = customName; // copy custom name
                            body.AddBodyPart(bodyPartCustomName, ref bodyNode);
                        }
                    }

                    // write back to body
                    body.SetNode(bodyNode);
                }

                AvailableBodies[bodyTypeName] = body;
            }
        }
    }
}
